using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using WebApp;
using WebApp.Controllers;

namespace Alba.Testing.Acceptance
{
    public class using_custom_service_registrations
    {
        [Fact]
        public async Task override_service_registration_in_bootstrapping()
        {
            ValuesController.LastWidget = Array.Empty<IWidget>();

            using var system = await AlbaHost.For<Startup>(builder =>
                {
                    builder.ConfigureServices((c, _) =>
                    {
                        _.AddTransient<IWidget, RedWidget>();
                    });
                });

            ValuesController.LastWidget = null;

            // The default registration is a GreenWidget

            await system.Scenario(_ =>
            {

                _.Put.Url("/api/values/foo").ContentType("application/json");
            });

            ValuesController.LastWidget.Length.ShouldBe(2);
        }

        [Fact]
        public async Task can_request_services()
        {
            using var system = await AlbaHost.For<Startup>(builder =>
            {
                builder.ConfigureServices((c, _) => { _.AddHttpContextAccessor(); });
            });

            var accessor1 = system.Services.GetService<IHttpContextAccessor>();
            Assert.NotNull(accessor1);

            var accessor2 = system.Server.Services.GetService<IHttpContextAccessor>();
            Assert.NotNull(accessor2);

            var accessor3 = ((IAlbaHost)system).Services.GetService<IHttpContextAccessor>();
            Assert.NotNull(accessor3);
        }
    }

}