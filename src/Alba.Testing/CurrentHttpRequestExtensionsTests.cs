using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class CurrentHttpRequestExtensionsTester
    {
        private IDictionary<string, object> theRequest = new Dictionary<string, object>();


        [Fact]
        public void is_get()
        {
            theRequest.HttpMethod("get");
            theRequest.IsGet().ShouldBeTrue();

            theRequest.HttpMethod("GET");
            theRequest.IsGet().ShouldBeTrue();

            theRequest.HttpMethod("POST");
            theRequest.IsGet().ShouldBeFalse();
        }

        [Fact]
        public void is_head()
        {
            theRequest.HttpMethod("head");
            theRequest.IsHead().ShouldBeTrue();

            theRequest.HttpMethod("HEAD");
            theRequest.IsHead().ShouldBeTrue();

            theRequest.HttpMethod("POST");
            theRequest.IsHead().ShouldBeFalse();
        }

        [Fact]
        public void is_post()
        {
            theRequest.HttpMethod("post");
            theRequest.IsPost().ShouldBeTrue();

            theRequest.HttpMethod("POST");
            theRequest.IsPost().ShouldBeTrue();

            theRequest.HttpMethod("get");
            theRequest.IsPost().ShouldBeFalse();

        }

        [Fact]
        public void is_put()
        {
            theRequest.HttpMethod("put");
            theRequest.IsPut().ShouldBeTrue();

            theRequest.HttpMethod("PUT");
            theRequest.IsPut().ShouldBeTrue();

            theRequest.HttpMethod("get");
            theRequest.IsPut().ShouldBeFalse();
        }

        [Fact]
        public void relative_url()
        {
            theRequest.RelativeUrl("");
            theRequest.ToRelativeContentUrl("/foo")
                .ShouldBe("foo");

            theRequest.RelativeUrl("/bar");
            theRequest.ToRelativeContentUrl("/foo")
                .ShouldBe("../foo");

            theRequest.RelativeUrl("/bar");
            theRequest.ToRelativeContentUrl("/bar/1")
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