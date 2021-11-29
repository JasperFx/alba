using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Alba.Testing.Acceptance
{
#if NET6_0_OR_GREATER
    public class web_application_factory_usage
    {

        [Fact]
        public async Task handle_blow_ups()
        {
            // WebApplicationFactory can resolve old and new style of Program.cs
            // .NET 6 style - the global:: namespace prefix would not be required in a normal test project
            await using var host = await AlbaHost.For<global::Program>(x =>
            {
                x.ConfigureServices((context, services) =>
                {
                    //...
                });
            });

            var ex = await Should.ThrowAsync<ScenarioAssertionException>(async () =>
            {
                await host.Scenario(x =>
                {
                    x.Get.Url("/blowup");
                });
            });

            ex.Message.ShouldContain("Expected status code 200, but was 500");
        }
        public interface IService{}
        public class ServiceA : IService{}

        [Fact]
        public async Task bootstrapping_with_WebApplicationFactory()
        {
            // WebApplicationFactory can resolve old and new style of Program.cs
            // .NET 6 style - the global:: namespace prefix would not be required in a normal test project
            await using var host = await AlbaHost.For<global::Program>(x =>
            {
                x.ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IService, ServiceA>();
                });
            });

            host.Services.GetRequiredService<IService>().ShouldBeOfType<ServiceA>();

            var text = await host.GetAsText("/");
            text.ShouldBe("Hello World!");

            // Works with .NET 5 / Traditional entry point
            await using var host2 = await AlbaHost.For<WebApp.Program>(x =>
            {
                x.ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IService, ServiceA>();
                });
            });
            
            host2.Services.GetRequiredService<IService>().ShouldBeOfType<ServiceA>();

            var text2 = await host2.GetAsText("/api/values");
            text2.ShouldBe("value1, value2");
        }
    }
#endif
}