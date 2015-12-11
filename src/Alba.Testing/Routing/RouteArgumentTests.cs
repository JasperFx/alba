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
        public void happy_path_with_number()
        {
            var env = new Dictionary<string, object>();

            var parameter = new RouteArgument("foo", 1);
            parameter.ArgType = typeof (int);

            parameter.SetValues(env, "a/25/c/d".Split('/'));

            env.GetRouteData("foo").ShouldBe(25);
        }

        [Fact]
        public void canonical_path()
        {
            var parameter = new RouteArgument("foo", 1);
            parameter.CanonicalPath().ShouldBe("*");
        }

        [Fact]
        public void the_default_arg_type_is_string()
        {
            var parameter = new RouteArgument("foo", 1);
            parameter.ArgType.ShouldBe(typeof(string));
        }

        [Fact]
        public void can_override_the_parameter_arg_type()
        {
            var parameter = new RouteArgument("foo", 1);
            parameter.ArgType = typeof (int);

            parameter.ArgType.ShouldBe(typeof(int));
        }
    }
}