using System.Text.RegularExpressions;
using Alba.Assertions;
using Microsoft.AspNetCore.Http;

namespace Alba.Testing.Assertions
{
    public class HeaderMatchAssertionTests
    {
        private readonly HeaderMatchAssertion _assertion;

        public HeaderMatchAssertionTests()
        {
            _assertion = new HeaderMatchAssertion("foo", new Regex("^b.?r$"));
        }

        [Fact]
        public void happy_path()
        {
            AssertionRunner.Run(_assertion, x => x.Response.Headers["foo"] = "bar")
                .AssertAll();
        }

        [Fact]
        public void sad_path_no_values()
        {
            AssertionRunner.Run(_assertion, e => { })
                .SingleMessageShouldBe("Expected a single header value of 'foo' matching '^b.?r$', but no values were found on the response");
        }

        [Fact]
        public void sad_path_wrong_value()
        {
            AssertionRunner.Run(_assertion, x => x.Response.Headers["foo"] = "baz")
                .SingleMessageShouldBe("Expected a single header value of 'foo' matching '^b.?r$', but the actual value was 'baz'");
        }

        [Fact]
        public void sad_path_too_many_values()
        {
            AssertionRunner.Run(_assertion, x =>
                {
                    x.Response.Headers.Append("foo", "baz");
                    x.Response.Headers.Append("foo", "bar");
                })
                .SingleMessageShouldBe("Expected a single header value of 'foo' matching '^b.?r$', but the actual values were 'baz', 'bar'");
        }
    }
}
