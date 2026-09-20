#nullable enable
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace Alba.Testing;

public class fluent_bootstrapping
{
    private static IHostBuilder inlineHost()
    {
        return new HostBuilder().ConfigureWebHost(x =>
        {
            x.Configure(app => app.Run(c => c.Response.WriteAsync("ok")));
        });
    }

    [Fact]
    public async Task with_host_builder()
    {
        var extension = new FakeExtension();

        await using var host = await AlbaHost.For(inlineHost())
            .WithConfiguration("Alba:Fluent", "from-builder")
            .WithExtension(extension);

        assertApplied(host, extension);
    }

    [Fact]
    public async Task with_web_application_builder()
    {
        var extension = new FakeExtension();

        await using var host = await WebApplication.CreateBuilder()
            .StartAlbaAsync(app => { })
            .WithConfiguration("Alba:Fluent", "from-builder")
            .WithExtension(extension);

        assertApplied(host, extension);
    }

    [Fact]
    public async Task with_web_application_factory()
    {
        var extension = new FakeExtension();

        await using var host = await AlbaHost.For<WebApp.Program>()
            .WithConfiguration("Alba:Fluent", "from-builder")
            .WithExtension(extension);

        assertApplied(host, extension);
    }

    [Fact]
    public async Task combined_with_the_web_host_configuration_action()
    {
        await using var host = await AlbaHost.For<WebApp.Program>(x =>
            {
                x.ConfigureServices(s => s.AddSingleton(new Marker("from-action")));
            })
            .WithConfiguration("Alba:Fluent", "from-builder");

        host.Services.GetRequiredService<Marker>().Name.ShouldBe("from-action");
        host.Services.GetRequiredService<IConfiguration>()["Alba:Fluent"].ShouldBe("from-builder");
    }

    [Fact]
    public async Task configuration_overrides_beat_the_applications_own_sources_on_host_builder()
    {
        var builder = new HostBuilder()
            .ConfigureAppConfiguration(c => c.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Alba:Source"] = "application"
            }))
            .ConfigureWebHost(x =>
            {
                x.Configure(app => app.Run(c => c.Response.WriteAsync("ok")));
            });

        await using var host = await AlbaHost.For(builder).WithConfiguration("Alba:Source", "alba");

        host.Services.GetRequiredService<IConfiguration>()["Alba:Source"].ShouldBe("alba");
    }

    [Fact]
    public async Task configuration_values_can_be_supplied_together()
    {
        await using var host = await AlbaHost.For(inlineHost())
            .WithConfiguration(new Dictionary<string, string?>
            {
                ["Alba:One"] = "1",
                ["Alba:Two"] = "2"
            });

        var configuration = host.Services.GetRequiredService<IConfiguration>();
        configuration["Alba:One"].ShouldBe("1");
        configuration["Alba:Two"].ShouldBe("2");
    }

    [Fact]
    public async Task extensions_are_applied_in_the_order_they_are_chained()
    {
        await using var host = await AlbaHost.For(inlineHost())
            .WithExtension(new MarkerExtension("first"))
            .WithExtension(new MarkerExtension("second"));

        host.Services.GetRequiredService<Marker>().Name.ShouldBe("second");
    }

    [Fact]
    public async Task awaiting_the_builder_twice_returns_the_same_host()
    {
        var builder = AlbaHost.For(inlineHost());

        await using var first = await builder;
        var second = await builder;

        second.ShouldBeSameAs(first);
    }

    [Fact]
    public async Task configuring_after_the_host_started_is_rejected()
    {
        var builder = AlbaHost.For(inlineHost());
        await using var host = await builder;

        Should.Throw<InvalidOperationException>(() => builder.WithConfiguration("Alba:Late", "value"));
        Should.Throw<InvalidOperationException>(() => builder.WithExtension(new FakeExtension()));
    }

    [Fact]
    public async Task configuration_is_readable_by_startup_code_before_build()
    {
        await using var host = await AlbaHost.For<MinimalApiWithOakton.Program>()
            .WithConfiguration("Alba:Greeting", "Hello from Alba");

        (await host.GetAsText("/greeting")).ShouldBe("Hello from Alba");
    }

#if NET11_0_OR_GREATER
    [Fact]
    public async Task configuration_reaches_an_application_that_ignores_its_args()
    {
        await using var host = await AlbaHost.For<MinimalApiWithoutArgs.Program>()
            .WithConfiguration("Alba:Greeting", "Hello from Alba");

        (await host.GetAsText("/greeting")).ShouldBe("Hello from Alba");
    }

    [Fact]
    public async Task configuration_does_not_travel_as_command_line_args()
    {
        await using var host = await AlbaHost.For<MinimalApiWithOakton.Program>()
            .WithConfiguration("Alba:Greeting", "Hello from Alba");

        var result = await host.Scenario(x => x.Get.Url("/args"));
        var args = await result.ReadAsJsonAsync<string[]>();

        args.Select(x => x.Split('=')[0])
            .ShouldBe(["--environment", "--contentRoot", "--applicationName"], ignoreOrder: true);
    }

    [Fact]
    public async Task startup_class_applications_are_rejected_by_the_web_application_factory()
    {
        var ex = await Should.ThrowAsync<InvalidOperationException>(
            async () => await AlbaHost.For<WebApiStartupHostingModel.Program>());

        ex.Message.ShouldContain("ConfigureWebApplicationBuilder");
    }
#endif

    private static void assertApplied(IAlbaHost host, FakeExtension extension)
    {
        host.Services.GetRequiredService<IConfiguration>()["Alba:Fluent"].ShouldBe("from-builder");

        extension.WasConfigured.ShouldBeTrue();
        extension.WasStarted.ShouldBeTrue();
    }

    private record Marker(string Name);

    private class MarkerExtension : IAlbaExtension
    {
        private readonly string _name;

        public MarkerExtension(string name)
        {
            _name = name;
        }

        public void Dispose()
        {
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public Task Start(IAlbaHost host) => Task.CompletedTask;

        public void Configure(IAlbaHostBuilder builder)
        {
            builder.ConfigureServices(s => s.AddSingleton(new Marker(_name)));
        }
    }
}
