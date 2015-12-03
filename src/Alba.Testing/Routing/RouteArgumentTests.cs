using System.Collections.Generic;
using Alba.Routing;
using Shouldly;
using Xunit;

namespace Alba.Testing.Routing
{
    public class RouteArgumentTests
    {
        [Fact]
        public void happy_path()
        {
            var env = new Dictionary<string, object>();

            var parameter = new RouteArgument("foo", 1);

            parameter.SetValues(env, "a/b/c/d".Split('/'));

            env.GetRouteData("foo").ShouldBe("b");
        }

        [Fact]
        public void canonical_path()
        {
            var parameter = new RouteArgument("foo", 1);
            parameter.CanonicalPath().ShouldBe("*");
        }
    }
}