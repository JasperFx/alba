using Alba.Assertions;
using Microsoft.AspNetCore.Http;

namespace Alba.Testing.Assertions
{
    public class HasSingleHeaderValueAssertionTests
    {
        [Fact]
        public void happy_path()
        {
            var assertion = new HasSingleHeaderValueAssertion("foo");
            AssertionRunner.Run(assertion, x => x.Response.Headers["foo"] = "bar")
                .AssertAll();
        }

        [Fact]
        public void sad_path_no_values()
        {
            var assertion = new HasSingleHeaderValueAssertion("foo");
            AssertionRunner.Run(assertion, e => { })
                .SingleMessageShouldBe("Expected a single header value of 'foo', but no values were found on the response");
        }

        [Fact]
        public void sad_path_too_many_values()
        {
            var assertion = new HasSingleHeaderValueAssertion("foo");
            AssertionRunner.Run(assertion, x =>
            {
                x.Response.Headers.Append("foo", "baz");
                x.Response.Headers.Append("foo", "bar");
            })
                .SingleMessageShouldBe("Expected a single header value of 'foo', but found multiple values on the response: 'baz', 'bar'");
        }
    }
}