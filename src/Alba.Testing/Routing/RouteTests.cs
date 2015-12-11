using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alba.Routing;
using Shouldly;
using Xunit;

namespace Alba.Testing.Routing
{
    public class RouteTests
    {
        [Fact]
        public void blank_segment()
        {
            Route.ToParameter("foo", 0).ShouldBeOfType<Alba.Routing.Segment>().Path.ShouldBe("foo");
        }

        [Fact]
        public void spread()
        {
            Route.ToParameter("...", 4).ShouldBeOfType<Spread>()
                .Position.ShouldBe(4);
        }

        [Fact]
        public void argument_starting_with_colon()
        {
            var arg = Route.ToParameter(":foo", 2).ShouldBeOfType<RouteArgument>();
            arg.Position.ShouldBe(2);
            arg.Key.ShouldBe("foo");
        }

        [Fact]
        public void argument_in_brackets()
        {
            var arg = Route.ToParameter("{bar}", 3).ShouldBeOfType<RouteArgument>();
            arg.Position.ShouldBe(3);
            arg.Key.ShouldBe("bar");
        }

        [Fact]
        public void spread_has_to_be_last()
        {
            Action action = () =>
            {
                new Route("a/.../b", "GET", env => Task.CompletedTask);
            };
            action.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void cannot_have_multiple_spreads_either()
        {
            Action action = () =>
            {
                new Route("a/.../b/...", "GET", env => Task.CompletedTask);
            };

            action.ShouldThrow<ArgumentOutOfRangeException>();
        }


    }

    public class building_a_route_from_segments_Tests
    {
        private ISegment[] segments;
        private Route route;

        public building_a_route_from_segments_Tests()
        {
            segments = new ISegment[]
            {new Segment("folder", 0), new Segment("folder2", 1), new RouteArgument("name", 2)};


            route = new Route(segments, HttpVerbs.PUT, env => Task.CompletedTask);

        }

        [Fact]
        public void should_have_the_appfunc()
        {
            route.AppFunc.ShouldNotBeNull();
        }

        [Fact]
        public void should_build_the_pattern_from_the_segments()
        {
            route.Pattern.ShouldBe("folder/folder2/:name");
        }

        [Fact]
        public void should_remember_the_httpMethod()
        {
            route.HttpMethod.ShouldBe(HttpVerbs.PUT);
        }

        [Fact]
        public void still_has_the_original_segments()
        {
            route.Segments.ShouldHaveTheSameElementsAs(segments);
        }

        [Fact]
        public void still_derives_the_name()
        {
            route.Name.ShouldBe("folder/folder2/:name");
        }

        [Fact]
        public void parameters_still_work()
        {
            var environment = new Dictionary<string, object>();

            route.SetValues(environment, new string[] { "folder", "folder2", "somebody" });

            environment.GetRouteData("name").ShouldBe("somebody");
        }
    }
}