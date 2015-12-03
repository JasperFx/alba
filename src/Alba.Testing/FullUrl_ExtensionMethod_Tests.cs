using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class FullUrl_ExtensionMethod_Tests
    {
        private IDictionary<string, object> environment;
        private IDictionary<string, string[]> headers;

        public FullUrl_ExtensionMethod_Tests()
        {
            environment = new Dictionary<string, object>();
            headers = new Dictionary<string, string[]>();
            environment.Add(OwinConstants.RequestHeadersKey, headers);
            environment.Add(OwinConstants.RequestSchemeKey, "https");

            environment.RequestHeaders().Replace("Host", "localhost");
        }


        private void setHeader(string header, string value)
        {
            headers[header] = new[] { value };
        }

        [Fact]
        public void should_prepend_scheme()
        {
            environment.FullUrl().ShouldStartWith("https://");
        }

        [Fact]
        public void should_support_host_without_port()
        {
            environment.FullUrl().ShouldStartWith("https://localhost");
        }

        [Fact]
        public void should_support_host_with_port()
        {
            setHeader("Host", "localhost:8080");
            environment.FullUrl().ShouldStartWith("https://localhost:8080");
        }

        [Fact]
        public void should_support_ip_address_with_port()
        {
            setHeader("Host", "127.0.0.1:8080");
            environment.FullUrl().ShouldStartWith("https://127.0.0.1:8080");
        }

        [Fact]
        public void should_be_tolerant_of_invalid_host_format()
        {
            setHeader("Host", "localhost:  ");
            environment.FullUrl().ShouldStartWith("https://localhost");
        }

        [Fact]
        public void should_ignore_invalid_port()
        {
            setHeader("Host", "localhost:a");
            environment.FullUrl().ShouldStartWith("https://localhost");
        }

        [Fact]
        public void should_use_path()
        {
            environment[OwinConstants.RequestPathKey] = "/foo";
            environment.FullUrl().ShouldBe("https://localhost/foo");
        }

        [Fact]
        public void should_support_querystring()
        {
            environment[OwinConstants.RequestQueryStringKey] = "baz=foo";
            environment.FullUrl().ShouldBe("https://localhost/?baz=foo");
        }

        [Fact]
        public void should_support_path_and_querystring()
        {
            environment[OwinConstants.RequestPathKey] = "/foo";
            environment[OwinConstants.RequestQueryStringKey] = "baz=foo";
            environment.FullUrl().ShouldBe("https://localhost/foo?baz=foo");
        }
    }
}