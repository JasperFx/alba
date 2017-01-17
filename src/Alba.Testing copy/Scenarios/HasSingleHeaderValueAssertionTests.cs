using Alba.Scenarios;
using Alba.Testing.Scenarios.Assertions;
using Xunit;

namespace Alba.Testing.Scenarios
{
    public class HasSingleHeaderValueAssertionTests
    {
        [Fact]
        public void happy_path()
        {
            var assertion = new HasSingleHeaderValueAssertion("foo");
            AssertionRunner.Run(assertion, x => x.ResponseHeaders().Replace("foo", "bar"))
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
                x.ResponseHeaders().Append("foo", "baz");
                x.ResponseHeaders().Append("foo", "bar");
            })
                .SingleMessageShouldBe("Expected a single header value of 'foo', but found multiple values on the response: 'baz', 'bar'");
        }
    }
}