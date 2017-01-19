using System.Collections.Generic;
using System.Linq;
using Alba.Stubs;
using Shouldly;
using Xunit;

namespace AlbaRouter.Testing
{
    public class EnvironmentExtensionsTests
    {
        private readonly StubHttpContext theContext = StubHttpContext.Empty();
            
            
            
        [Fact]
        public void get_route_data_from_null_state()
        {
            theContext.GetRouteData("foo").ShouldBeNull();
        }

        [Fact]
        public void get_route_data_miss()
        {
            theContext.Items.Add(EnvironmentExtensions.RouteData, new Dictionary<string, object>());

            theContext.GetRouteData("foo").ShouldBeNull();
        }

        [Fact]
        public void get_route_data_hit()
        {
            theContext.Items.Add(EnvironmentExtensions.RouteData, new Dictionary<string, object> { {"foo", "bar"} });

            theContext.GetRouteData("foo").ShouldBe("bar");
        }

        [Fact]
        public void set_route_data()
        {
            var dict = new Dictionary<string, object> { {"foo", "bar"} };
            theContext.SetRouteData(dict);

            theContext.GetRouteData("foo").ShouldBe("bar");
        }

        [Fact]
        public void get_route_data_dictionary_from_empty_state()
        {
            theContext.GetRouteData().Keys.Any().ShouldBeFalse();
        }

        [Fact]
        public void get_route_data_from_environment()
        {
            var routeValues = new Dictionary<string, object> { { "foo", "bar" } };
            theContext.Items.Add(EnvironmentExtensions.RouteData, routeValues);

            theContext.GetRouteData().ShouldBeSameAs(routeValues);
        }

        [Fact]
        public void get_spread_data_from_empty()
        {
            theContext.GetSpreadData().Length.ShouldBe(0);
        }

        [Fact]
        public void get_spread_data_from_env()
        {
            var spread = new[] {"a", "b", "c"};
            theContext.Items.Add(EnvironmentExtensions.SpreadData, spread);

            theContext.GetSpreadData().ShouldBeSameAs(spread);
        }

        [Fact]
        public void set_spread_data()
        {
            var spread = new[] { "a", "b", "c" };
            theContext.SetSpreadData(spread);

            theContext.Items[EnvironmentExtensions.SpreadData].ShouldBeSameAs(spread);


        }
    }
}