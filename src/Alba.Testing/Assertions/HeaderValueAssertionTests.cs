using Alba.Assertions;
using Microsoft.AspNetCore.Http;

namespace Alba.Testing.Assertions
{
    public class HeaderValueAssertionTests
    {
        [Fact]
        public void happy_path()
        {
            var assertion = new HeaderValueAssertion("foo", "bar");
            AssertionRunner.Run(assertion, x => x.Response.Headers["foo"] = "bar")
                .AssertAll();
        }

        [Fact]
        public void sad_path_no_values()
        {
            var assertion = new HeaderValueAssertion("foo", "bar");
            AssertionRunner.Run(assertion, e => {})
                .SingleMessageShouldBe("Expected a single header value of 'foo'='bar', but no values were found on the response");
        }

        [Fact]
        public void sad_path_wrong_value()
        {
            var assertion = new HeaderValueAssertion("foo", "bar");
            AssertionRunner.Run(assertion, x => x.Response.Headers["foo"] = "baz")
                .SingleMessageShouldBe("Expected a single header value of 'foo'='bar', but the actual value was 'baz'");
        }

        [Fact]
        public void sad_path_too_many_values()
        {
            var assertion = new HeaderValueAssertion("foo", "bar");
            AssertionRunner.Run(assertion, x =>
            {
                x.Response.Headers.Append("foo", "baz");
                x.Response.Headers.Append("foo", "bar");
            })
            .SingleMessageShouldBe("Expected a single header value of 'foo'='bar', but the actual values were 'baz', 'bar'");
        }
    }
}