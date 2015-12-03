using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class HttpMethodExtensionsTests
    {
        private IDictionary<string, object> theEnvironment = new Dictionary<string, object>();


        [Fact]
        public void get_http_method()
        {
            theEnvironment.Add(OwinConstants.RequestMethodKey, "GET");

            theEnvironment.HttpMethod().ShouldBe("GET");
        }

        [Fact]
        public void set_the_http_method()
        {
            theEnvironment.HttpMethod("POST");
            theEnvironment[OwinConstants.RequestMethodKey].ShouldBe("POST");
        }

        [Fact]
        public void http_method_matches_any()
        {
            theEnvironment.HttpMethod("HEAD");

            theEnvironment.HttpMethodMatchesAny("PUT", "POST").ShouldBeFalse();

            theEnvironment.HttpMethodMatchesAny("HEAD", "DELETE").ShouldBeTrue();
        }

        [Fact]
        public void is_not_http_method()
        {
            theEnvironment.HttpMethod("HEAD");

            theEnvironment.IsNotHttpMethod("PUT", "POST").ShouldBeTrue();

            theEnvironment.IsNotHttpMethod("HEAD", "DELETE").ShouldBeFalse();
        }


        [Fact]
        public void is_get()
        {
            theEnvironment.HttpMethod("get");
            theEnvironment.IsGet().ShouldBeTrue();

            theEnvironment.HttpMethod("GET");
            theEnvironment.IsGet().ShouldBeTrue();

            theEnvironment.HttpMethod("POST");
            theEnvironment.IsGet().ShouldBeFalse();
        }

        [Fact]
        public void is_head()
        {
            theEnvironment.HttpMethod("head");
            theEnvironment.IsHead().ShouldBeTrue();

            theEnvironment.HttpMethod("HEAD");
            theEnvironment.IsHead().ShouldBeTrue();

            theEnvironment.HttpMethod("POST");
            theEnvironment.IsHead().ShouldBeFalse();
        }

        [Fact]
        public void is_post()
        {
            theEnvironment.HttpMethod("post");
            theEnvironment.IsPost().ShouldBeTrue();

            theEnvironment.HttpMethod("POST");
            theEnvironment.IsPost().ShouldBeTrue();

            theEnvironment.HttpMethod("get");
            theEnvironment.IsPost().ShouldBeFalse();

        }

        [Fact]
        public void is_put()
        {
            theEnvironment.HttpMethod("put");
            theEnvironment.IsPut().ShouldBeTrue();

            theEnvironment.HttpMethod("PUT");
            theEnvironment.IsPut().ShouldBeTrue();

            theEnvironment.HttpMethod("get");
            theEnvironment.IsPut().ShouldBeFalse();
        }

        [Fact]
        public void relative_url()
        {
            theEnvironment.RelativeUrl("");
            theEnvironment.ToRelativeContentUrl("/foo")
                .ShouldBe("foo");

            theEnvironment.RelativeUrl("/bar");
            theEnvironment.ToRelativeContentUrl("/foo")
                .ShouldBe("../foo");

            theEnvironment.RelativeUrl("/bar");
            theEnvironment.ToRelativeContentUrl("/bar/1")
                .ShouldBe("1");


        }

        [Fact]
        public void get_comma_separated_values_from_header()
        {
            new[] { "v1", "v2, v3", "\"v4, b\"", "v5, v6", "v7", }
                .GetCommaSeparatedHeaderValues()
                .ShouldHaveTheSameElementsAs("v1", "v2", "v3", "v4, b", "v5", "v6", "v7");

            new[] { "v1,v2, v3,\"v4, b\",v5, v6,v7" }
                .GetCommaSeparatedHeaderValues()
                .ShouldHaveTheSameElementsAs("v1", "v2", "v3", "v4, b", "v5", "v6", "v7");
        }


        [Fact]
        public void etag_matches_with_no_values()
        {
            new string[0].EtagMatches("foo")
                .ShouldBe(EtagMatch.None);
        }

        [Fact]
        public void etag_matches_with_wildcard()
        {
            new string[] { "a", "*", "b" }
                .EtagMatches("foo")
                .ShouldBe(EtagMatch.Yes);
        }

        [Fact]
        public void etag_matches_positive()
        {
            new string[] { "a", "b", "foo" }
                .EtagMatches("foo")
                .ShouldBe(EtagMatch.Yes);
        }

        [Fact]
        public void etag_matches_negative()
        {
            new string[] { "a", "b", "bar" }
                .EtagMatches("foo")
                .ShouldBe(EtagMatch.No);
        }
    }
}