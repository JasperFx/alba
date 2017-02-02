using System.Linq;
using Alba.Stubs;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class StubHttpRequestTests
    {
        private readonly StubHttpRequest theRequest = new StubHttpRequest(StubHttpContext.Empty());

        [Fact]
        public void set_content_type_by_header()
        {
            theRequest.Headers["content-type"] = "application/json";

            theRequest.ContentType.ShouldBe("application/json");
        }

        [Fact]
        public void set_content_type_directly()
        {
            theRequest.ContentType = "text/plain";
            theRequest.Headers["content-type"][0].ShouldBe("text/plain");
        }

        [Fact]
        public void set_content_length_by_header()
        {
            theRequest.Headers["content-length"] = "55";

            theRequest.ContentLength.ShouldBe(55L);
        }

        [Fact]
        public void set_content_length_directly()
        {
            theRequest.ContentLength = 100;
            theRequest.Headers["content-length"][0].ShouldBe("100");
        }

        [Fact]
        public void empty_content_length()
        {
            theRequest.ContentLength.ShouldBeNull();
        }
    }
}