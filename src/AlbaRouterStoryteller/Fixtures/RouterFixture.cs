using System.Collections.Generic;
using System.Threading.Tasks;
using Alba;
using Alba.Stubs;
using AlbaRouter;
using Baseline;
using Microsoft.AspNetCore.Http;
using StoryTeller;
using StoryTeller.Grammars.Tables;

namespace AlbaRouterStoryteller.Fixtures
{
    public class RouterFixture : Fixture
    {
        private Router _router;

        public RouterFixture()
        {
            Title = "End to End Router";
        }

        public override void SetUp()
        {
            _router = new Router();
        }

        [ExposeAsTable("If the routes are")]
        public void RoutesAre([SelectionValues("GET", "POST", "DELETE", "PUT", "HEAD")]string HttpMethod, string Pattern)
        {
            _router.Add(HttpMethod, Pattern, env =>
            {
                env.Response.ContentType("text/plain");
                return env.Response.WriteAsync($"{HttpMethod}: /{Pattern}");
            });
        }

        [ExposeAsTable("The selection and arguments should be")]
        public void TheResultShouldBe([SelectionValues("GET", "POST", "DELETE", "PUT", "HEAD")]string HttpMethod, string Url, out int Status, out string Body, [Default("NONE")]out ArgumentExpectation Arguments)
        {
            var context = StubHttpContext.Empty();
            context.RelativeUrl(Url);
            context.HttpMethod(HttpMethod);

            _router.Invoke(context).Wait();

            Status = context.Response.StatusCode;

            context.Response.Body.Position = 0;
            Body = context.Response.Body.ReadAllText();
            Arguments = new ArgumentExpectation(context);
        }

    }
}