using Microsoft.AspNetCore.Http;
using Shouldly;

namespace Alba.Testing.Acceptance
{
    public class assertions_against_the_querystring : ScenarioContext
    {
        [Fact]
        public Task using_scenario_with_QueryString_should_set_query_string_parameter()
        {
            router.Handlers["/one"] = c =>
            {
                c.Response.StatusCode = 200;
                return c.Response.WriteAsync(c.Request.Query["test"]);
            };

            return host.Scenario(_ =>
            {
                _.Get.Url("/one").QueryString("test", "value");
                
                _.ConfigureHttpContext(c =>
                {
                    c.Request.Query["test"].ToString().ShouldBe("value");
                });
            });
        }
    }
}