#nullable enable
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace Alba.Testing
{
    public class using_extensions_with_async_builder
    {
        private readonly FakeExtension extension1;
        private readonly FakeExtension extension2;
        private readonly FakeExtension extension3;
        private readonly IHostBuilder theBuilder;
        private readonly IAlbaHost theHost;

        public using_extensions_with_async_builder()
        {
            extension1 = new FakeExtension();
            extension2 = new FakeExtension();
            extension3 = new FakeExtension();

            theBuilder = Host.CreateDefaultBuilder();

            theHost = theBuilder.StartAlbaAsync(extension1, extension2, extension3).GetAwaiter().GetResult();
        }

        [Fact]
        public void all_extensions_should_be_applied_to_configure()
        {
            extension1.WasConfigured.ShouldBeTrue();
            extension2.WasConfigured.ShouldBeTrue();
            extension3.WasConfigured.ShouldBeTrue();

            theHost.Dispose();
        }

        [Fact]
        public void all_extensions_should_be_started()
        {
            extension1.WasStarted.ShouldBeTrue();
            extension2.WasStarted.ShouldBeTrue();
            extension3.WasStarted.ShouldBeTrue();

            theHost.Dispose();
        }

        [Fact]
        public async Task all_extensions_disposed_async()
        {
            await theHost.DisposeAsync();
            
            extension1.WasAsyncDisposed.ShouldBeTrue();
            extension2.WasAsyncDisposed.ShouldBeTrue();
            extension3.WasAsyncDisposed.ShouldBeTrue();
        }
        
        [Fact]
        public void sync_dispose_disposes_extensions_asynchronously()
        {
            theHost.Dispose();

            extension1.WasAsyncDisposed.ShouldBeTrue();
            extension2.WasAsyncDisposed.ShouldBeTrue();
            extension3.WasAsyncDisposed.ShouldBeTrue();
        }
    }


    public class FakeExtension : IAlbaExtension
    {
        public void Dispose()
        {
            WasDisposed = true;
        }

        public bool WasDisposed { get; set; }

        public ValueTask DisposeAsync()
        {
            WasAsyncDisposed = true;
            return ValueTask.CompletedTask;
        }

        public bool WasAsyncDisposed { get; set; }

        public Task Start(IAlbaHost host)
        {
            WasStarted = true;
            return Task.CompletedTask;
        }

        public bool WasStarted { get; set; }

        public void Configure(IAlbaHostBuilder builder)
        {
            WasConfigured = true;
        }

        public bool WasConfigured { get; set; }
    }

    public class extensions_behave_identically_across_bootstrapping_styles
    {
        [Fact]
        public async Task with_host_builder()
        {
            var builder = new HostBuilder().ConfigureWebHost(x =>
            {
                x.Configure(app => app.Run(c => c.Response.WriteAsync("ok")));
            });

            await using var host = await AlbaHost.For(builder, new ServiceAndConfigExtension());

            assertExtensionWasApplied(host);
        }

        [Fact]
        public async Task with_web_application_builder()
        {
            await using var host = await WebApplication.CreateBuilder()
                .StartAlbaAsync(app => { }, new ServiceAndConfigExtension());

            assertExtensionWasApplied(host);
        }

        [Fact]
        public async Task with_web_application_factory()
        {
            await using var host = await AlbaHost.For<WebApp.Program>(new ServiceAndConfigExtension());

            assertExtensionWasApplied(host);
        }

        [Fact]
        public async Task configuration_override_beats_existing_appsettings_value_on_minimal_hosting_under_web_application_factory()
        {
            // MinimalApiWithOakton's appsettings.json sets Logging:LogLevel:Default to "Information";
            // this is the dotnet/aspnetcore#37680 scenario the ConfigurationOverride extension exists for
            var overrides = ConfigurationOverride.Create(new Dictionary<string, string?>
            {
                ["Logging:LogLevel:Default"] = "Critical"
            });

            await using var host = await AlbaHost.For<MinimalApiWithOakton.Program>(overrides);

            var configuration = host.Services.GetRequiredService<IConfiguration>();
            configuration["Logging:LogLevel:Default"].ShouldBe("Critical");
        }

        private static void assertExtensionWasApplied(IAlbaHost host)
        {
            host.Services.GetRequiredService<MarkerService>().ShouldNotBeNull();

            var configuration = host.Services.GetRequiredService<IConfiguration>();
            configuration["Alba:ExtensionValue"].ShouldBe("from-extension");
        }

        private class ServiceAndConfigExtension : IAlbaExtension
        {
            public void Dispose()
            {
            }

            public ValueTask DisposeAsync() => ValueTask.CompletedTask;

            public Task Start(IAlbaHost host) => Task.CompletedTask;

            public void Configure(IAlbaHostBuilder builder)
            {
                builder.ConfigureServices(s => s.AddSingleton(new MarkerService()));
                builder.ConfigureConfiguration(c => c.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Alba:ExtensionValue"] = "from-extension"
                }));
            }
        }

        private class MarkerService
        {
        }
    }
}