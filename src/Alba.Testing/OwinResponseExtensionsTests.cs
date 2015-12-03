using System.Collections.Generic;
using System.Linq;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class OwinResponseExtensionsTests
    {
        [Fact]
        public void set_and_read_request_id()
        {
            var dict = new Dictionary<string, object>();
            dict.RequestId().ShouldBeNull();

            dict.RequestId("abc");

            dict.RequestId().ShouldBe("abc");

            dict.ResponseHeaders()[OwinConstants.REQUEST_ID].Single().ShouldBe("abc");
        }


        [Fact]
        public void response_headers_when_none()
        {
            var dict = new Dictionary<string, object>();
            var headers = dict.ResponseHeaders();
            headers.ShouldNotBeNull();

            dict[OwinConstants.ResponseHeadersKey].ShouldBeSameAs(headers);
        }

        [Fact]
        public void existing_request_headers()
        {
            var dict = new Dictionary<string, object>();
            var headers = new Dictionary<string, string[]>();
            dict.Add(OwinConstants.ResponseHeadersKey, headers);

            dict.ResponseHeaders().ShouldBeSameAs(headers);
        }

    }
}