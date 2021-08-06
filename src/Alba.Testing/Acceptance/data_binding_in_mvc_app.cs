using System.Collections.Generic;
using System.Threading.Tasks;
using Shouldly;
using WebApp;
using WebApp.Controllers;
using Xunit;

namespace Alba.Testing.Acceptance
{
    public class data_binding_in_mvc_app
    {
// SAMPLE: binding-against-a-model
        [Fact]
        public async Task can_bind_to_form_data()
        {

            using (var system = AlbaTestHost.ForStartup<Startup>())
            {

                var input = new InputModel {
                    One = "one",
                    Two = "two",
                    Three = "three"
                };

                await system.Scenario(_ =>
                {
                    _.Post.FormData(input)
                        .ToUrl("/gateway/insert");
                });


                GatewayController.LastInput.ShouldNotBeNull();

                GatewayController.LastInput.One.ShouldBe("one");
                GatewayController.LastInput.Two.ShouldBe("two");
                GatewayController.LastInput.Three.ShouldBe("three");
            }
        }

// ENDSAMPLE

        [Fact]
        public async Task can_bind_to_form_data_as_dictionary()
        {
            using (var system = AlbaTestHost.ForStartup<Startup>())
            {
                var dict = new Dictionary<string, string> {{"One", "one"}, {"Two", "two"}, {"Three", "three"}};


                await system.Scenario(_ =>
                {
                    _.Post.FormData(dict)
                        .ToUrl("/gateway/insert");
                });

                GatewayController.LastInput.ShouldNotBeNull();

                GatewayController.LastInput.One.ShouldBe("one");
                GatewayController.LastInput.Two.ShouldBe("two");
                GatewayController.LastInput.Three.ShouldBe("three");
            }
        }
    }
}