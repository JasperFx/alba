using System.Net.ServerSentEvents;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MinimalApiWithOakton;
using Shouldly;

namespace Alba.Testing.Sse;

public class streaming_sse : IAsyncLifetime
{
    private IAlbaHost _host = null!;

    public async ValueTask InitializeAsync()
    {
        _host = await AlbaHost.For<MinimalApiWithOakton.Program>();
    }

    public async ValueTask DisposeAsync()
    {
        await _host.DisposeAsync();
    }

    private static CancellationTokenSource testTimeout() => new(TimeSpan.FromSeconds(10));

    [Fact]
    public async Task can_stream_a_finite_event_stream()
    {
        using var timeout = testTimeout();

        await using var stream = await _host.StreamServerSentEvents(x => x.Get.Url("/sse/finite"), timeout.Token);

        var events = new List<SseItem<string>>();
        await foreach (var item in stream.ReadEvents(timeout.Token))
        {
            events.Add(item);
        }

        events.Count.ShouldBe(3);
        events[0].EventType.ShouldBe("count");
        events[0].EventId.ShouldBe("1");
        events[2].Data.ShouldBe("{\"number\":3}");
    }

    [Fact]
    public async Task can_stream_typed_events()
    {
        using var timeout = testTimeout();

        await using var stream = await _host.StreamServerSentEvents(x => x.Get.Url("/sse/finite"), timeout.Token);

        var numbers = new List<int>();
        await foreach (var item in stream.ReadEvents<CounterEvent>(timeout.Token))
        {
            numbers.Add(item.Data.Number);
        }

        numbers.ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public async Task disposing_the_stream_aborts_the_request_on_the_server()
    {
        using var timeout = testTimeout();

        var tracker = _host.Services.GetRequiredService<SseAbortTracker>();
        tracker.Reset();

        var stream = await _host.StreamServerSentEvents(x => x.Get.Url("/sse/infinite"), timeout.Token);

        var count = 0;
        await foreach (var _ in stream.ReadEvents(timeout.Token))
        {
            if (++count == 2) break;
        }

        await stream.DisposeAsync();

        await tracker.Aborted.WaitAsync(timeout.Token);
    }

    [Fact]
    public async Task before_each_async_actions_apply_to_streams()
    {
        await using var host = await AlbaHost.For<MinimalApiWithOakton.Program>();
        host.BeforeEachAsync(s =>
        {
            s.WithRequestHeader("x-sse-echo", "from-async-hook");
            return Task.CompletedTask;
        });

        using var timeout = testTimeout();
        await using var stream = await host.StreamServerSentEvents(x => x.Get.Url("/sse/echo-header"), timeout.Token);

        var events = new List<string>();
        await foreach (var item in stream.ReadEvents(timeout.Token))
        {
            events.Add(item.Data);
        }

        events.ShouldBe(new[] { "from-async-hook" });
    }

    [Fact]
    public async Task synchronous_before_each_actions_apply_to_streams()
    {
        await using var host = await AlbaHost.For<MinimalApiWithOakton.Program>();
        host.BeforeEach(c => c.Request.Headers["x-sse-echo"] = "from-sync-hook");

        using var timeout = testTimeout();
        await using var stream = await host.StreamServerSentEvents(x => x.Get.Url("/sse/echo-header"), timeout.Token);

        var events = new List<string>();
        await foreach (var item in stream.ReadEvents(timeout.Token))
        {
            events.Add(item.Data);
        }

        events.ShouldBe(new[] { "from-sync-hook" });
    }

    [Fact]
    public async Task after_each_actions_run_when_the_stream_is_disposed()
    {
        await using var host = await AlbaHost.For<MinimalApiWithOakton.Program>();

        var called = false;
        HttpContext observed = new DefaultHttpContext();
        host.AfterEachAsync(c =>
        {
            called = true;
            observed = c;
            return Task.CompletedTask;
        });

        using var timeout = testTimeout();
        var stream = await host.StreamServerSentEvents(x => x.Get.Url("/sse/finite"), timeout.Token);

        called.ShouldBeFalse();

        await stream.DisposeAsync();

        called.ShouldBeTrue();
        observed.ShouldBeNull();
    }

    [Fact]
    public async Task non_sse_endpoints_are_rejected()
    {
        var ex = await Should.ThrowAsync<InvalidOperationException>(
            () => _host.StreamServerSentEvents(x => x.Get.Url("/")));

        ex.Message.ShouldContain("text/event-stream");
    }

    [Fact]
    public async Task response_assertions_are_rejected()
    {
        var ex = await Should.ThrowAsync<InvalidOperationException>(
            () => _host.StreamServerSentEvents(x =>
            {
                x.Get.Url("/sse/finite");
                x.ContentShouldContain("number");
            }));

        ex.Message.ShouldContain("Response assertions are not supported");
    }

    [Fact]
    public async Task missing_url_is_rejected()
    {
        var ex = await Should.ThrowAsync<InvalidOperationException>(
            () => _host.StreamServerSentEvents(_ => { }));

        ex.Message.ShouldBe("This scenario has no defined url");
    }

    [Fact]
    public async Task the_event_stream_can_only_be_read_once()
    {
        using var timeout = testTimeout();

        await using var stream = await _host.StreamServerSentEvents(x => x.Get.Url("/sse/finite"), timeout.Token);

        await foreach (var _ in stream.ReadEvents(timeout.Token))
        {
        }

        Should.Throw<InvalidOperationException>(() => stream.ReadEvents());
    }
}
