using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class OwinUrlExtensionsTests
    {
        private IDictionary<string, object> theEnvironment = new Dictionary<string, object>();


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
        public void set_the_relative_url_without_querystring()
        {
            theEnvironment.RelativeUrl("/foo");

            theEnvironment[OwinConstants.RequestPathKey].ShouldBe("/foo");
            theEnvironment.ContainsKey(OwinConstants.RequestQueryStringKey).ShouldBeFalse();
        }

        [Fact]
        public void set_the_relative_url_with_query_string()
        {
            theEnvironment.RelativeUrl("/planets/hoth?temp=cold");

            theEnvironment[OwinConstants.RequestPathKey].ShouldBe("/planets/hoth");
            theEnvironment[OwinConstants.RequestQueryStringKey].ShouldBe("temp=cold");
        }

        [Fact]
        public void retrieve_the_relative_url_without_query_string()
        {
            theEnvironment.Add(OwinConstants.RequestPathKey, "/planets/hoth");

            theEnvironment.RelativeUrl().ShouldBe("/planets/hoth");
        }

        [Fact]
        public void retrieve_the_relative_url_with_query_string()
        {
            theEnvironment.Add(OwinConstants.RequestPathKey, "/planets/hoth");
            theEnvironment.Add(OwinConstants.RequestQueryStringKey, "temp=cold");

            theEnvironment.RelativeUrl().ShouldBe("/planets/hoth?temp=cold");
        }


        [Fact]
        public void server_root_round_trip()
        {
            theEnvironment.FullUrl("http://server/foo/bar");
            theEnvironment.FullUrl().ShouldBe("http://server/foo/bar");

            theEnvironment.RelativeUrl().ShouldBe("/foo/bar");
        }


        [Fact]
        public void server_root_round_trip_with_querystring()
        {
            theEnvironment.FullUrl("http://server/foo/bar?foo=bar");
            theEnvironment.FullUrl().ShouldBe("http://server/foo/bar?foo=bar");

            theEnvironment.RelativeUrl().ShouldBe("/foo/bar?foo=bar");
        }

        [Fact]
        public void set_the_entire_full_url()
        {
            theEnvironment.FullUrl("https://server/foo/bar?foo=bar");

            theEnvironment[OwinConstants.RequestSchemeKey].ShouldBe("https");
            theEnvironment[OwinConstants.RequestPathBaseKey].ShouldBe(string.Empty); // it's just assumed
            theEnvironment.RequestHeaders().Get("Host").ShouldBe("server");
            theEnvironment[OwinConstants.RequestPathKey].ShouldBe("/foo/bar");
            theEnvironment[OwinConstants.RequestQueryStringKey].ShouldBe("foo=bar");
        }

        [Fact]
        public void to_rull_url_when_it_is_already_a_full_url()
        {
            theEnvironment.ToFullUrl("http://localhost/go").ShouldBe("http://localhost/go");
        }

        [Fact]
        public void to_full_url_without_request_path_base()
        {
            theEnvironment.FullUrl("https://server/");
            theEnvironment.ToFullUrl("/foo/bar?what=this").ShouldBe("/foo/bar?what=this");
        }

        [Fact]
        public void to_full_url_without_request_path_base_2()
        {
            theEnvironment.FullUrl("https://server/");
            theEnvironment.ToFullUrl("~/foo/bar?what=this").ShouldBe("/foo/bar?what=this");
        }

        [Fact]
        public void to_full_url_without_request_path_base_3()
        {
            theEnvironment.FullUrl("https://server/");
            theEnvironment.ToFullUrl("foo/bar?what=this").ShouldBe("/foo/bar?what=this");
        }

        [Fact]
        public void to_rull_url_with_request_path_base()
        {
            theEnvironment.FullUrl("https://server");
            theEnvironment[OwinConstants.RequestPathBaseKey] = "myapp";


            theEnvironment.ToFullUrl("foo").ShouldBe("/myapp/foo");
            theEnvironment.ToFullUrl("/foo").ShouldBe("/myapp/foo");
            theEnvironment.ToFullUrl("~/foo").ShouldBe("/myapp/foo");
            theEnvironment.ToFullUrl("/foo?bar=one").ShouldBe("/myapp/foo?bar=one");
            theEnvironment.ToFullUrl("/foo?bar=one&temp=chilly").ShouldBe("/myapp/foo?bar=one&temp=chilly");
        }
    }
}