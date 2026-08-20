using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace Alba.Testing.Acceptance;

public class scenario_setup_failures : IAsyncLifetime
{
    private IAlbaHost _host = null!;
    private int _invocations;

    public async ValueTask InitializeAsync()
    {
        var builder = new HostBuilder().ConfigureWebHost(x =>
        {
            x.Configure(app => app.Run(c =>
            {
                Interlocked.Increment(ref _invocations);
                return c.Response.WriteAsync("hello");
            }));
        });

        _host = await AlbaHost.For(builder);
    }

    public async ValueTask DisposeAsync()
    {
        await _host.DisposeAsync();
    }

    [Fact]
    public async Task setup_exception_is_rethrown_with_its_original_stack_trace()
    {
        var ex = await Should.ThrowAsync<InvalidOperationException>(() => _host.Scenario(x =>
        {
            x.Get.Url("/");
            x.ConfigureHttpContext(_ => throw new InvalidOperationException("I blew up"));
        }));

        ex.Message.ShouldBe("I blew up");
        ex.StackTrace.ShouldContain(nameof(setup_exception_is_rethrown_with_its_original_stack_trace));
    }

    [Fact]
    public async Task the_application_is_not_invoked_when_the_setup_fails()
    {
        await Should.ThrowAsync<InvalidOperationException>(() => _host.Scenario(x =>
        {
            x.Get.Url("/");
            x.ConfigureHttpContext(_ => throw new InvalidOperationException("I blew up"));
        }));

        _invocations.ShouldBe(0);
    }

    [Fact]
    public async Task missing_url_does_not_invoke_the_application()
    {
        var ex = await Should.ThrowAsync<InvalidOperationException>(() => _host.Scenario(_ => { }));

        ex.Message.ShouldBe("This scenario has no defined url");
        _invocations.ShouldBe(0);
    }
}
