using System.Threading.Tasks;
using Xunit;

namespace Alba.Testing.Acceptance
{
    public class route_lookup_invocations : ScenarioContext
    {

        [Fact]
        public Task using_scenario_with_controller_expression()
        {
            router.RegisterRoute<InMemoryEndpoint>(e => e.get_memory_hello(), "GET", "/memory/hello");

            router.Handlers["/memory/hello"] = c =>
            {
                c.Response.ContentType("text/plain");
                c.Response.Write("hello from the in memory host");

                return Task.CompletedTask;
            };

            return host.Scenario(x =>
            {
                x.Get.Action<InMemoryEndpoint>(e => e.get_memory_hello());
                x.ContentShouldBe("hello from the in memory host");
            });
        }


    }
}