using System.Diagnostics;
using Alba.Extensions;
using Alba.Pooling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace Alba.Testing.Acceptance;

public class host_pooling
{
    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private static AlbaHostBuilder startApp(AlbaHostSlot slot, params IAlbaExtension[] extensions)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddHttpClient("payments", c => c.BaseAddress = new Uri("https://payments.example.com"));

        return AlbaHost.For(builder, app =>
            {
                app.MapGet("/config/{key}", (string key, IConfiguration config) => config[key]);
                app.MapGet("/header/{name}", (string name, HttpContext c) => c.Request.Headers[name].ToString());
            }, extensions)
            .WithConfiguration("Slot", slot.Index.ToString());
    }

    private static async Task<IAlbaHost> failToStart(Task gate)
    {
        await gate;
        throw new FormatException("The application failed to start");
    }

    private static HttpClient payments(IAlbaHost host)
    {
        return host.Services.GetRequiredService<IHttpClientFactory>().CreateClient("payments");
    }

    private sealed class Resource : IAsyncDisposable
    {
        public bool Disposed { get; private set; }

        public ValueTask DisposeAsync()
        {
            Disposed = true;
            return ValueTask.CompletedTask;
        }
    }

    // Each host the pool starts registers a resource with its slot, in the order the hosts start
    private static Func<AlbaHostSlot, AlbaHostBuilder> trackingStarts(List<Resource> started)
    {
        return slot =>
        {
            var resource = new Resource();
            lock (started) started.Add(resource);
            slot.RegisterForDispose(resource);
            return startApp(slot);
        };
    }

    [Fact]
    public async Task a_returned_host_is_reused()
    {
        var started = new List<Resource>();
        await using var pool = new AlbaHostPool(trackingStarts(started));

        IServiceProvider first;
        await using (var host = await pool.RentAsync(Cancellation)) first = host.Services;

        await using (var host = await pool.RentAsync(Cancellation))
        {
            host.Services.ShouldBeSameAs(first);
            host.Slot.Index.ShouldBe(0);
        }

        started.Count.ShouldBe(1);
    }

    [Fact]
    public async Task hosts_rented_at_once_get_their_own_slot_and_configuration()
    {
        await using var pool = new AlbaHostPool(slot => startApp(slot), new AlbaHostPoolOptions { MaxHosts = 2 });

        await using var first = await pool.RentAsync(Cancellation);
        await using var second = await pool.RentAsync(Cancellation);

        (await first.GetAsText("/config/Slot")).ShouldBe("0");
        (await second.GetAsText("/config/Slot")).ShouldBe("1");
        second.Slot.Name.ShouldBe("alba-1");
    }

    [Fact]
    public async Task an_async_factory_can_prepare_resources_before_the_host_starts()
    {
        await using var pool = new AlbaHostPool(async (slot, _) =>
        {
            var database = new Resource();
            slot.RegisterForDispose(database);
            await Task.Yield();

            return await startApp(slot).WithConfiguration("Database", $"app_{slot.Index}");
        });

        await using var host = await pool.RentAsync(Cancellation);
        (await host.GetAsText("/config/Database")).ShouldBe("app_0");
    }

    [Fact]
    public async Task rents_wait_in_order_for_a_host_to_be_returned()
    {
        await using var pool = new AlbaHostPool(slot => startApp(slot), new AlbaHostPoolOptions { MaxHosts = 1 });

        var first = await pool.RentAsync(Cancellation);
        var second = pool.RentAsync(Cancellation);
        var third = pool.RentAsync(Cancellation);

        await Task.Delay(50, Cancellation);
        second.IsCompleted.ShouldBeFalse();

        var services = first.Services;
        await first.DisposeAsync();

        var lease = await second;
        lease.Services.ShouldBeSameAs(services);
        third.IsCompleted.ShouldBeFalse();

        await lease.DisposeAsync();
        await using var last = await third;
        last.Services.ShouldBeSameAs(services);
    }

    [Fact]
    public async Task parallel_tests_never_hold_more_hosts_than_the_limit()
    {
        var started = new List<Resource>();
        var held = 0;
        var mostHeld = 0;
        await using var pool = new AlbaHostPool(trackingStarts(started), new AlbaHostPoolOptions { MaxHosts = 3 });

        await Task.WhenAll(Enumerable.Range(0, 30).Select(async _ =>
        {
            await using var host = await pool.RentAsync(Cancellation);

            var now = Interlocked.Increment(ref held);
            lock (pool) mostHeld = Math.Max(mostHeld, now);

            await host.GetAsText("/config/Slot");
            Interlocked.Decrement(ref held);
        }));

        mostHeld.ShouldBeLessThanOrEqualTo(3);
        started.Count.ShouldBeLessThanOrEqualTo(3);
    }

    [Fact]
    public async Task a_long_queue_does_not_time_out_while_hosts_keep_coming_back()
    {
        await using var pool = new AlbaHostPool(slot => startApp(slot),
            new AlbaHostPoolOptions { MaxHosts = 1, RentTimeout = TimeSpan.FromMilliseconds(500) });

        // The last rent waits far longer than the timeout, but a host comes back every 60ms
        await Task.WhenAll(Enumerable.Range(0, 15).Select(async _ =>
        {
            await using var host = await pool.RentAsync(Cancellation);
            await Task.Delay(60, Cancellation);
        }));
    }

    [Fact]
    public async Task a_rent_times_out_naming_the_leases_not_returned()
    {
        await using var pool = new AlbaHostPool(slot => startApp(slot),
            new AlbaHostPoolOptions { MaxHosts = 1, RentTimeout = TimeSpan.FromMilliseconds(100) });

        await using var held = await pool.RentAsync(Cancellation);

        var ex = await Should.ThrowAsync<TimeoutException>(pool.RentAsync(Cancellation));
        ex.Message.ShouldContain("all 1 hosts are in use");
        ex.Message.ShouldContain($"alba-0 rented by {nameof(a_rent_times_out_naming_the_leases_not_returned)} (host_pooling.cs:");
    }

    [Fact]
    public async Task a_cancelled_rent_stops_waiting()
    {
        await using var pool = new AlbaHostPool(slot => startApp(slot), new AlbaHostPoolOptions { MaxHosts = 1 });

        var held = await pool.RentAsync(Cancellation);
        var services = held.Services;

        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(Cancellation);
        var rent = pool.RentAsync(cancellation.Token);
        await cancellation.CancelAsync();
        await Should.ThrowAsync<OperationCanceledException>(rent);

        // The returned host goes back to the pool rather than to the cancelled rent
        await held.DisposeAsync();
        await using var next = await pool.RentAsync(Cancellation);
        next.Services.ShouldBeSameAs(services);
    }

    [Fact]
    public async Task a_startup_failure_fails_later_rents_without_starting_more_hosts()
    {
        var attempts = 0;
        await using var pool = new AlbaHostPool(_ =>
        {
            Interlocked.Increment(ref attempts);
            throw new FormatException("The application failed to start");
        });

        var failure = await Should.ThrowAsync<FormatException>(pool.RentAsync(Cancellation));

        var later = await Should.ThrowAsync<InvalidOperationException>(pool.RentAsync(Cancellation));
        later.InnerException.ShouldBeSameAs(failure);
        await Should.ThrowAsync<InvalidOperationException>(pool.WarmUpAsync(cancellationToken: Cancellation));

        attempts.ShouldBe(1);
    }

    [Fact]
    public async Task a_startup_failure_fails_the_rents_waiting_for_that_host()
    {
        var gate = new TaskCompletionSource();
        await using var pool = new AlbaHostPool((_, _) => failToStart(gate.Task),
            new AlbaHostPoolOptions { MaxHosts = 1 });

        var starting = pool.RentAsync(Cancellation);
        var waiting = pool.RentAsync(Cancellation);
        gate.SetResult();

        await Should.ThrowAsync<FormatException>(starting);
        (await Should.ThrowAsync<InvalidOperationException>(waiting)).InnerException.ShouldBeOfType<FormatException>();
    }

    [Fact]
    public async Task without_fail_fast_each_rent_tries_to_start_a_host()
    {
        var attempts = 0;
        await using var pool = new AlbaHostPool(_ =>
        {
            Interlocked.Increment(ref attempts);
            throw new FormatException("The application failed to start");
        }, new AlbaHostPoolOptions { FailFastOnStartupFailure = false });

        await Should.ThrowAsync<FormatException>(pool.RentAsync(Cancellation));
        await Should.ThrowAsync<FormatException>(pool.RentAsync(Cancellation));

        attempts.ShouldBe(2);
    }

    [Fact]
    public async Task on_created_runs_once_per_host_and_on_rent_runs_for_every_rent()
    {
        var created = 0;
        var rented = 0;
        await using var pool = new AlbaHostPool(slot => startApp(slot), new AlbaHostPoolOptions
        {
            OnCreated = (_, _) =>
            {
                created++;
                return Task.CompletedTask;
            },
            OnRent = (_, _) =>
            {
                rented++;
                return Task.CompletedTask;
            }
        });

        await (await pool.RentAsync(Cancellation)).DisposeAsync();
        await (await pool.RentAsync(Cancellation)).DisposeAsync();

        created.ShouldBe(1);
        rented.ShouldBe(2);
    }

    [Fact]
    public async Task a_failed_on_rent_replaces_the_host()
    {
        var failRent = true;
        await using var pool = new AlbaHostPool(slot => startApp(slot), new AlbaHostPoolOptions
        {
            MaxHosts = 1,
            OnRent = (_, _) => failRent ? throw new FormatException("Seeding failed") : Task.CompletedTask
        });

        await Should.ThrowAsync<FormatException>(pool.RentAsync(Cancellation));

        failRent = false;
        await using var host = await pool.RentAsync(Cancellation);
        host.Slot.Index.ShouldBe(0);
    }

    [Fact]
    public async Task returning_a_host_runs_on_return_and_a_failed_reset_replaces_the_host()
    {
        var resources = new List<Resource>();
        var resets = 0;
        var failReset = false;

        await using var pool = new AlbaHostPool(trackingStarts(resources), new AlbaHostPoolOptions
        {
            MaxHosts = 1,
            OnReturn = (_, _) =>
            {
                resets++;
                return failReset ? throw new FormatException("The reset failed") : Task.CompletedTask;
            }
        });

        await (await pool.RentAsync(Cancellation)).DisposeAsync();
        resets.ShouldBe(1);

        failReset = true;
        var lease = await pool.RentAsync(Cancellation);
        var replaced = lease.Services;
        await Should.ThrowAsync<FormatException>(lease.DisposeAsync().AsTask());
        resources[0].Disposed.ShouldBeTrue();

        failReset = false;
        await using var next = await pool.RentAsync(Cancellation);
        next.Slot.Index.ShouldBe(0);
        next.Services.ShouldNotBeSameAs(replaced);
        resources.Count.ShouldBe(2);
    }

    [Fact]
    public async Task a_discarded_host_is_replaced_in_the_same_slot()
    {
        var resources = new List<Resource>();
        var resets = 0;

        await using var pool = new AlbaHostPool(trackingStarts(resources), new AlbaHostPoolOptions
        {
            MaxHosts = 1,
            OnReturn = (_, _) =>
            {
                resets++;
                return Task.CompletedTask;
            }
        });

        var lease = await pool.RentAsync(Cancellation);
        var discarded = lease.Services;
        lease.Discard();
        await lease.DisposeAsync();

        resets.ShouldBe(0);
        resources[0].Disposed.ShouldBeTrue();

        await using var next = await pool.RentAsync(Cancellation);
        next.Slot.Index.ShouldBe(0);
        next.Services.ShouldNotBeSameAs(discarded);
    }

    [Fact]
    public async Task actions_added_through_a_lease_end_when_it_is_returned()
    {
        var afterEach = 0;
        await using var pool = new AlbaHostPool(slot => startApp(slot), new AlbaHostPoolOptions
        {
            MaxHosts = 1,
            OnCreated = (host, _) =>
            {
                host.BeforeEach(c => c.Request.Headers["x-created"] = "every test");
                return Task.CompletedTask;
            },
            OnRent = (host, _) =>
            {
                host.AfterEach(_ => afterEach++);
                return Task.CompletedTask;
            }
        });

        await using (var host = await pool.RentAsync(Cancellation))
        {
            host.BeforeEachAsync(s =>
            {
                s.WithRequestHeader("x-async", "leased");
                return Task.CompletedTask;
            });
            host.BeforeEach(c => c.Request.Headers["x-sync"] = "leased");

            (await host.GetAsText("/header/x-async")).ShouldBe("leased");
            (await host.GetAsText("/header/x-sync")).ShouldBe("leased");
        }

        await using (var host = await pool.RentAsync(Cancellation))
        {
            (await host.GetAsText("/header/x-async")).ShouldBeEmpty();
            (await host.GetAsText("/header/x-sync")).ShouldBeEmpty();
            (await host.GetAsText("/header/x-created")).ShouldBe("every test");
        }

        // The action added in OnRent runs once per scenario, rather than piling up on each rent
        afterEach.ShouldBe(5);
    }

    [Fact]
    public async Task returning_a_host_restores_the_responses_its_http_client_stub_started_with()
    {
        await using var pool = new AlbaHostPool(slot => startApp(slot,
                new HttpClientStub("payments").When(HttpMethod.Get, "/status").RespondWithText("up")),
            new AlbaHostPoolOptions
            {
                MaxHosts = 1,
                OnCreated = (host, _) =>
                {
                    host.GetExtension<HttpClientStub>().When(HttpMethod.Get, "/version").RespondWithText("1.0");
                    return Task.CompletedTask;
                }
            });

        await using (var host = await pool.RentAsync(Cancellation))
        {
            var stub = host.GetExtension<HttpClientStub>();
            stub.When(HttpMethod.Get, "/status").RespondWithText("down");

            (await payments(host).GetStringAsync("/status", Cancellation)).ShouldBe("down");
            stub.Requests.Count.ShouldBe(1);
        }

        await using (var host = await pool.RentAsync(Cancellation))
        {
            var stub = host.GetExtension<HttpClientStub>();
            stub.Requests.ShouldBeEmpty();

            (await payments(host).GetStringAsync("/status", Cancellation)).ShouldBe("up");
            (await payments(host).GetStringAsync("/version", Cancellation)).ShouldBe("1.0");
        }
    }

    [Fact]
    public async Task a_resettable_extension_cannot_be_shared_between_hosts()
    {
        var shared = new HttpClientStub("payments");
        await using var pool = new AlbaHostPool(slot => startApp(slot, shared), new AlbaHostPoolOptions { MaxHosts = 2 });

        await using var first = await pool.RentAsync(Cancellation);

        var ex = await Should.ThrowAsync<InvalidOperationException>(pool.RentAsync(Cancellation));
        ex.Message.ShouldStartWith("This HttpClientStub is already used by another host in the AlbaHostPool");
    }

    [Fact]
    public async Task endpoint_coverage_is_shared_by_every_host_in_the_pool()
    {
        var coverage = new EndpointCoverage();
        await using var pool = new AlbaHostPool(slot => startApp(slot, coverage), new AlbaHostPoolOptions { MaxHosts = 2 });

        await using var first = await pool.RentAsync(Cancellation);
        await using var second = await pool.RentAsync(Cancellation);

        await first.GetAsText("/config/Slot");
        await second.GetAsText("/header/x-test");

        coverage.NotExercised().ShouldBeEmpty();
    }

    [Fact]
    public async Task get_extension_finds_one_extension_of_the_type()
    {
        await using var pool = new AlbaHostPool(slot =>
            startApp(slot, new HttpClientStub("payments"), new HttpClientStub("email")));

        await using var host = await pool.RentAsync(Cancellation);

        host.GetExtension<HttpClientStub>(x => x.ClientName == "email").ClientName.ShouldBe("email");

        Should.Throw<InvalidOperationException>(() => host.GetExtension<HttpClientStub>())
            .Message.ShouldBe("The host has 2 HttpClientStub extensions. Pass a match to choose one.");
        Should.Throw<InvalidOperationException>(() => host.GetExtension<TimeProviderOverride>())
            .Message.ShouldBe("The host has no TimeProviderOverride extension");
    }

    [Fact]
    public async Task a_returned_lease_can_no_longer_be_used()
    {
        await using var pool = new AlbaHostPool(slot => startApp(slot));

        var lease = await pool.RentAsync(Cancellation);
        Should.Throw<InvalidOperationException>(() => lease.StopAsync(Cancellation));

        await lease.DisposeAsync();
        await lease.DisposeAsync();

        Should.Throw<ObjectDisposedException>(() => lease.Services);
        await Should.ThrowAsync<ObjectDisposedException>(lease.GetAsText("/config/Slot"));
    }

    [Fact]
    public async Task disposing_the_pool_disposes_idle_hosts_and_hosts_when_they_are_returned()
    {
        var resources = new List<Resource>();
        var pool = new AlbaHostPool(trackingStarts(resources), new AlbaHostPoolOptions { MaxHosts = 2 });

        var rented = await pool.RentAsync(Cancellation);
        await (await pool.RentAsync(Cancellation)).DisposeAsync();

        await pool.DisposeAsync();
        resources[1].Disposed.ShouldBeTrue();
        resources[0].Disposed.ShouldBeFalse();

        await Should.ThrowAsync<ObjectDisposedException>(pool.RentAsync(Cancellation));

        await rented.DisposeAsync();
        resources[0].Disposed.ShouldBeTrue();
    }

    [Fact]
    public async Task warming_up_starts_hosts_ahead_of_the_first_rents()
    {
        var started = new List<Resource>();
        await using var pool = new AlbaHostPool(trackingStarts(started), new AlbaHostPoolOptions { MaxHosts = 3 });

        await pool.WarmUpAsync(2, Cancellation);
        started.Count.ShouldBe(2);

        await using var first = await pool.RentAsync(Cancellation);
        await using var second = await pool.RentAsync(Cancellation);
        started.Count.ShouldBe(2);

        // Rented hosts count as running, so this only starts the third
        await pool.WarmUpAsync(cancellationToken: Cancellation);
        started.Count.ShouldBe(3);
    }

    private sealed class Instance(AlbaHostSlot slot, List<string> teardown) : IAsyncDisposable
    {
        public AlbaHostSlot Slot { get; } = slot;

        public int Resets { get; set; }

        public ValueTask DisposeAsync()
        {
            lock (teardown) teardown.Add("state");
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task a_pool_can_keep_state_of_its_own_with_each_host()
    {
        var teardown = new List<string>();
        var pool = new AlbaHostPool<Instance>(async (slot, _) =>
        {
            var host = await startApp(slot);
            host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopped
                .Register(() => { lock (teardown) teardown.Add("host"); });
            slot.RegisterForDispose(new TeardownStep(() => { lock (teardown) teardown.Add("slot resource"); }));

            return (host, new Instance(slot, teardown));
        }, new AlbaHostPoolOptions<Instance>
        {
            MaxHosts = 1,
            OnReturn = (_, instance) =>
            {
                instance.Resets++;
                return Task.CompletedTask;
            }
        });

        Instance first;
        await using (var host = await pool.RentAsync(Cancellation))
        {
            first = host.State;
            first.Slot.ShouldBeSameAs(host.Slot);
        }

        var lease = await pool.RentAsync(Cancellation);
        lease.State.ShouldBeSameAs(first);
        first.Resets.ShouldBe(1);
        await lease.DisposeAsync();
        Should.Throw<ObjectDisposedException>(() => lease.State);

        await pool.DisposeAsync();
        teardown.ShouldBe(["state", "host", "slot resource"]);
    }

    private sealed class TeardownStep(Action step) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            step();
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task max_concurrent_starts_limits_how_many_hosts_start_at_once()
    {
        var starting = 0;
        var mostStarting = 0;
        await using var pool = new AlbaHostPool(async (slot, _) =>
        {
            var now = Interlocked.Increment(ref starting);
            lock (this) mostStarting = Math.Max(mostStarting, now);

            await Task.Delay(50, Cancellation);
            Interlocked.Decrement(ref starting);

            return await startApp(slot);
        }, new AlbaHostPoolOptions { MaxHosts = 3, MaxConcurrentStarts = 1 });

        await pool.WarmUpAsync(cancellationToken: Cancellation);

        mostStarting.ShouldBe(1);
    }

    [Fact]
    public async Task a_replacement_host_knows_its_generation()
    {
        var generations = new List<int>();
        await using var pool = new AlbaHostPool(slot =>
        {
            generations.Add(slot.Generation);
            return startApp(slot);
        }, new AlbaHostPoolOptions { MaxHosts = 1 });

        for (var i = 0; i < 2; i++)
        {
            var lease = await pool.RentAsync(Cancellation);
            lease.Discard();
            await lease.DisposeAsync();
        }

        await using var host = await pool.RentAsync(Cancellation);
        host.Slot.Generation.ShouldBe(2);
        generations.ShouldBe([0, 1, 2]);
    }

    [Fact]
    public async Task the_pool_traces_starting_renting_and_returning_hosts()
    {
        using var testSource = new ActivitySource(nameof(host_pooling));
        var recorded = new List<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name is "Alba" || source == testSource,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity =>
            {
                lock (recorded) recorded.Add(activity);
            }
        };
        ActivitySource.AddActivityListener(listener);

        await using var pool = new AlbaHostPool(slot => startApp(slot));

        ActivityTraceId traceId;
        using (var test = testSource.StartActivity("test")!)
        {
            traceId = test.TraceId;
            await (await pool.RentAsync(Cancellation)).DisposeAsync();
        }

        Activity[] pooling;
        lock (recorded)
        {
            pooling = recorded.Where(x => x.TraceId == traceId && x.DisplayName.StartsWith("AlbaHostPool")).ToArray();
        }

        pooling.Select(x => x.DisplayName).ShouldBe(["AlbaHostPool start host", "AlbaHostPool rent", "AlbaHostPool return"]);
        pooling.ShouldAllBe(x => Equals(x.GetTagItem("alba.pool.slot"), 0));
        pooling[1].GetTagItem("alba.pool.wait_ms").ShouldNotBeNull();
    }
}
