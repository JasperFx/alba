using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class OwinQueryStringExtensionsTests
    {
        [Fact]
        public void set_the_query_string()
        {
            var env = new Dictionary<string, object>();
            env.QueryString("?bar=1&foo=2");

            env[OwinConstants.RequestQueryStringKey].ShouldBe("bar=1&foo=2");
        }

        [Fact]
        public void get_raw_query_string()
        {
            var env = new Dictionary<string, object>();
            env.Add(OwinConstants.RequestQueryStringKey, "bar=1&foo=2");

            env.QueryString().ShouldBe("bar=1&foo=2");
        }

        [Fact]
        public void parse_the_query_string()
        {
            var env = new Dictionary<string, object>();
            env.QueryString("?bar=1&foo=2");

            var values = env.ParseQueryString();
            values["bar"].ShouldBe("1");
            values["foo"].ShouldBe("2");
        }
    }
}