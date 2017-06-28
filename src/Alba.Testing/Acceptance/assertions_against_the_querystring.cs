using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Alba.Testing.Acceptance
{
    public class assertions_against_the_querystring : ScenarioContext
    {
        [Fact]
        public Task using_scenario_with_QueryString_should_set_query_string_parameter()
        {
            host.Handlers["/one"] = c =>
            {
                c.Response.StatusCode = 200;
                return Task.CompletedTask;
            };

            return host.Scenario(_ =>
            {
                _.Get.QueryString("test", "value");

                _.Context.Request.Query["test"].ToString().ShouldBe("value");
            });
        }
    }
}