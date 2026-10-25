using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace Alba.Testing.Acceptance;

public class test_context_flow
{
    // Stands in for a test framework's current test, which is held in an AsyncLocal
    private static readonly AsyncLocal<string> CurrentTest = new();

    private sealed class SeenAtStartup
    {
        public string Test { get; set; } = "not started";
    }

    private sealed class CapturingHostedService(SeenAtStartup seen) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            seen.Test = CurrentTest.Value;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private static void captureStartup(IServiceCollection services, SeenAtStartup seen)
    {
        services.AddSingleton(seen);
        services.AddHostedService<CapturingHostedService>();
    }

    private static AlbaHostBuilder startApp()
    {
        return AlbaHost.For(WebApplication.CreateBuilder(), app =>
        {
            app.MapGet("/current-test", () => CurrentTest.Value ?? "none");
            app.MapGet("/traceparent", (HttpContext c) => c.Request.Headers.TraceParent.ToString());
        });
    }

    [Fact]
    public async Task hosted_services_start_without_the_callers_context_with_a_web_application_builder()
    {
        CurrentTest.Value = "the test that started the host";
        var seen = new SeenAtStartup();

        var builder = WebApplication.CreateBuilder();
        captureStartup(builder.Services, seen);
        await using var host = await AlbaHost.For(builder, _ => { });

        seen.Test.ShouldBeNull();
    }

    [Fact]
    public async Task hosted_services_start_without_the_callers_context_with_a_web_application_factory()
    {
        CurrentTest.Value = "the test that started the host";
        var seen = new SeenAtStartup();

        await using var host = await AlbaHost.For<global::Program>(x => x.ConfigureServices(s => captureStartup(s, seen)));

        seen.Test.ShouldBeNull();
    }

    [Fact]
    public async Task hosted_services_start_without_the_callers_context_with_a_host_builder()
    {
        CurrentTest.Value = "the test that started the host";
        var seen = new SeenAtStartup();

        var builder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(web => web.Configure(_ => { }))
            .ConfigureServices(s => captureStartup(s, seen));
        await using var host = await AlbaHost.For(builder);

        seen.Test.ShouldBeNull();
    }

    [Fact]
    public async Task requests_run_without_the_tests_context_by_default()
    {
        await using var host = await startApp();
        CurrentTest.Value = "sending test";

        await host.Scenario(x =>
        {
            x.Get.Url("/current-test");
            x.ContentShouldBe("none");
        });
    }

    [Fact]
    public async Task requests_run_in_the_tests_context_with_test_context_flow()
    {
        await using var host = await startApp().WithTestContextFlow();
        CurrentTest.Value = "sending test";

        await host.Scenario(x =>
        {
            x.Get.Url("/current-test");
            x.ContentShouldBe("sending test");
        });
    }

    [Fact]
    public async Task test_context_flow_applies_to_every_bootstrapping_style()
    {
        await using var factoryHost = await AlbaHost.For<global::Program>().WithTestContextFlow();
        await using var builderHost = await AlbaHost
            .For(Host.CreateDefaultBuilder().ConfigureWebHostDefaults(web => web.Configure(_ => { })))
            .WithTestContextFlow();

        factoryHost.Server.PreserveExecutionContext.ShouldBeTrue();
        builderHost.Server.PreserveExecutionContext.ShouldBeTrue();
    }

    [Fact]
    public async Task requests_carry_the_trace_context_of_the_tests_activity()
    {
        await using var host = await startApp();

        // A test framework's activity for the running test, with nothing listening to Alba's activities
        using var testActivity = new Activity("test").SetIdFormat(ActivityIdFormat.W3C).Start();

        var traceParent = await host.GetAsText("/traceparent");

        traceParent.ShouldContain(testActivity.TraceId.ToHexString());
    }
}
