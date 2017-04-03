using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using WebApp;
using WebApp.Controllers;
using Xunit;

namespace Alba.Testing.Acceptance
{
    public class using_custom_service_registrations
    {
        [Fact]
        public async Task override_service_registration_in_bootstrapping()
        {
            using (var system = SystemUnderTest.ForStartup<Startup>())
            {
                system.ConfigureServices(_ =>
                {
                    _.AddTransient<IWidget, RedWidget>();
                });

                ValuesController.LastWidget = null;

                // The default registration is a GreenWidget

                await system.Scenario(_ =>
                {
                    _.Get.Url("/api/values");
                });

                ValuesController.LastWidget.Length.ShouldBe(2);
            }
        }
    }

}