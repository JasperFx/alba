using Shouldly;
using WebApp;
using WebApp.Controllers;

namespace Alba.Testing.Acceptance
{
    public class data_binding_in_mvc_app
    {
#region sample_binding_against_a_model
        [Fact]
        public async Task can_bind_to_form_data()
        {
            await using var system = await AlbaHost.For<Startup>();

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

#endregion

        [Fact]
        public async Task can_bind_to_form_data_as_dictionary()
        {
            await using var system = await AlbaHost.For<Startup>();

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
