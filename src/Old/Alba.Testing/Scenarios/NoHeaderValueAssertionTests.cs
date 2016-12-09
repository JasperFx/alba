using Alba.Scenarios;
using Alba.Testing.Scenarios.Assertions;
using Xunit;

namespace Alba.Testing.Scenarios
{
    public class NoHeaderValueAssertionTests
    {
        [Fact]
        public void happy_path()
        {
            var assertion = new NoHeaderValueAssertion("foo");
            AssertionRunner.Run(assertion, e => {}).AssertAll();
        }

        [Fact]
        public void sad_path_any_values()
        {
            var assertion = new NoHeaderValueAssertion("foo");
            AssertionRunner.Run(assertion, x =>
            {
                x.ResponseHeaders().Append("foo", "baz");
                x.ResponseHeaders().Append("foo", "bar");
            })
                .SingleMessageShouldBe("Expected no value for header 'foo', but found values 'baz', 'bar'");
        }
    }
}