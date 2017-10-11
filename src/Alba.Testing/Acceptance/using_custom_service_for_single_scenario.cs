using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using WebApp;
using WebApp.Controllers;
using Xunit;

namespace Alba.Testing.Acceptance
{
    public class using_custom_service_for_single_scenario
    {
        [Fact]
        public async Task system_should_maintain_dependency_graph_after_scenario_is_run()
        {
            using (var system = SystemUnderTest.ForStartup<Startup>())
            {
                await system.Scenario(_ =>
                {
                    _.ConfigureServices(s =>
                    {
                        s.AddTransient<ISimpleService, TestService>();
                    });

                    _.Get.Url("/test-service");
                });

                system.Services.GetService<ISimpleService>().ShouldBeOfType<SimpleService>();
            }
        }

        [Fact]
        public async Task replace_service_per_test()
        {
            using (var system = SystemUnderTest.ForStartup<Startup>())
            {
                var result = await system.Scenario(_ =>
                {
                    _.ConfigureServices(x =>
                    {
                        x.AddTransient<ISimpleService, TestService>();
                    });

                    _.Get.Url("/test-service");
                });

                result.ResponseBody.ReadAsText().ShouldBe("replaced");
            }
        }
    }

    public class TestService : ISimpleService
    {
        public string Test()
        {
            return "replaced";
        }
    }
}