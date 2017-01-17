using Alba.Scenarios;
using Alba.Testing.Scenarios.Assertions;
using Xunit;

namespace Alba.Testing.Scenarios
{
    public class HeaderValueAssertionTests
    {
        [Fact]
        public void happy_path()
        {
            var assertion = new HeaderValueAssertion("foo", "bar");
            AssertionRunner.Run(assertion, x => x.ResponseHeaders().Replace("foo", "bar"))
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
            AssertionRunner.Run(assertion, x => x.ResponseHeaders().Replace("foo", "baz"))
                .SingleMessageShouldBe("Expected a single header value of 'foo'='bar', but the actual value was 'baz'");
        }

        [Fact]
        public void sad_path_too_many_values()
        {
            var assertion = new HeaderValueAssertion("foo", "bar");
            AssertionRunner.Run(assertion, x =>
            {
                x.ResponseHeaders().Append("foo", "baz");
                x.ResponseHeaders().Append("foo", "bar");
            })
            .SingleMessageShouldBe("Expected a single header value of 'foo'='bar', but the actual values were 'baz', 'bar'");
        }
    }
}