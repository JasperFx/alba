using System.Collections.Generic;
using System.Threading.Tasks;
using Alba.Routing;
using StoryTeller;
using StoryTeller.Grammars.Tables;

namespace Alba.Testing.Fixtures
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
                env.Write($"{HttpMethod}: /{Pattern}", "text/plain");
                return Task.CompletedTask;
            });
        }

        [ExposeAsTable("The selection and arguments should be")]
        public void TheResultShouldBe([SelectionValues("GET", "POST", "DELETE", "PUT", "HEAD")]string HttpMethod, string Url, out int Status, out string Body,[Default("NONE")]out ArgumentExpectation Arguments)
        {
            var env = new Dictionary<string, object>();
            env.RelativeUrl(Url);
            env.HttpMethod(HttpMethod);

            _router.Invoke(env).Wait();

            Status = env.StatusCode();
            Body = env.Response().ReadAsText();
            Arguments = new ArgumentExpectation(env);
        }

    }
}