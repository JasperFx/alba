using System.Collections.Generic;
using System.Linq;
using Alba.Routing;
using Shouldly;
using Xunit;

namespace Alba.Testing.Routing
{
    public class EnvironmentExtensionsTests
    {
        private readonly IDictionary<string, object> theEnvironment = new Dictionary<string, object>();
            
            
            
        [Fact]
        public void get_route_data_from_null_state()
        {
            theEnvironment.GetRouteData("foo").ShouldBeNull();
        }

        [Fact]
        public void get_route_data_miss()
        {
            theEnvironment.Add(EnvironmentExtensions.OwinRouteData, new Dictionary<string, object>());

            theEnvironment.GetRouteData("foo").ShouldBeNull();
        }

        [Fact]
        public void get_route_data_hit()
        {
            theEnvironment.Add(EnvironmentExtensions.OwinRouteData, new Dictionary<string, object> { {"foo", "bar"} });

            theEnvironment.GetRouteData("foo").ShouldBe("bar");
        }

        [Fact]
        public void set_route_data()
        {
            var dict = new Dictionary<string, object> { {"foo", "bar"} };
            theEnvironment.SetRouteData(dict);

            theEnvironment.GetRouteData("foo").ShouldBe("bar");
        }

        [Fact]
        public void get_route_data_dictionary_from_empty_state()
        {
            theEnvironment.GetRouteData().Keys.Any().ShouldBeFalse();
        }

        [Fact]
        public void get_route_data_from_environment()
        {
            var routeValues = new Dictionary<string, object> { { "foo", "bar" } };
            theEnvironment.Add(EnvironmentExtensions.OwinRouteData, routeValues);

            theEnvironment.GetRouteData().ShouldBeSameAs(routeValues);
        }

        [Fact]
        public void get_spread_data_from_empty()
        {
            theEnvironment.GetSpreadData().Length.ShouldBe(0);
        }

        [Fact]
        public void get_spread_data_from_env()
        {
            var spread = new[] {"a", "b", "c"};
            theEnvironment.Add(EnvironmentExtensions.OwinSpreadData, spread);

            theEnvironment.GetSpreadData().ShouldBeSameAs(spread);
        }

        [Fact]
        public void set_spread_data()
        {
            var spread = new[] { "a", "b", "c" };
            theEnvironment.SetSpreadData(spread);

            theEnvironment[EnvironmentExtensions.OwinSpreadData].ShouldBeSameAs(spread);


        }
    }
}