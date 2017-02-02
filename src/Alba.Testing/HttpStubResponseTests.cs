using Alba.Stubs;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class HttpStubResponseTests
    {
        private readonly StubHttpResponse theResponse = new StubHttpResponse(StubHttpContext.Empty());

        [Fact]
        public void initial_content_type()
        {
            theResponse.ContentType.ShouldBeNull();
        }

        [Fact]
        public void set_content_type_by_header()
        {
            theResponse.Headers["content-type"] = "application/json";

            theResponse.ContentType.ShouldBe("application/json");
        }

        [Fact]
        public void set_content_type_directly()
        {
            theResponse.ContentType = "text/plain";
            theResponse.Headers["content-type"][0].ShouldBe("text/plain");
        }

        [Fact]
        public void set_content_length_by_header()
        {
            theResponse.Headers["content-length"] = "55";

            theResponse.ContentLength.ShouldBe(55L);
        }

        [Fact]
        public void set_content_length_directly()
        {
            theResponse.ContentLength = 100;
            theResponse.Headers["content-length"][0].ShouldBe("100");
        }

        [Fact]
        public void empty_content_length()
        {
            theResponse.ContentLength.ShouldBeNull();
        }
    }
}