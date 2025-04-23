using Alba.Assertions;
using Microsoft.AspNetCore.Http;

namespace Alba.Testing.Assertions
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
                x.Response.Headers.Append("foo", "baz");
                x.Response.Headers.Append("foo", "bar");
            })
                .SingleMessageShouldBe("Expected no value for header 'foo', but found values 'baz', 'bar'");
        }
    }
}