using System.Threading.Tasks;
using Xunit;

namespace Alba.Testing.Acceptance
{
    public class route_lookup_invocations : ScenarioContext
    {

        [Fact]
        public Task using_scenario_with_controller_expression()
        {
            return host.Scenario(x =>
            {
                x.Get.Action<InMemoryEndpoint>(e => e.get_memory_hello());
                x.ContentShouldBe("hello from the in memory host");
            });
        }

        [Fact]
        public async Task using_scenario_with_input_model()
        {
            await host.Scenario(x =>
            {
                x.Get.Input(new InMemoryInput { Color = "Red" });
                x.ContentShouldBe("The color is Red");
            });


            await host.Scenario(x =>
            {
                x.Get.Input(new InMemoryInput { Color = "Orange" });
                x.ContentShouldBe("The color is Orange");
            });
        }


        [Fact]
        public Task using_scenario_with_input_model_as_marker()
        {
            return host.Scenario(x =>
            {
                x.Get.Input<MarkerInput>();
                x.ContentShouldBe("just the marker");
            });
        }


    }
}