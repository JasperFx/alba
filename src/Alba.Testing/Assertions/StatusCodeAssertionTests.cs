using Alba.Assertions;
using Xunit;

namespace Alba.Testing.Assertions
{
    public class StatusCodeAssertionTests
    {
        [Fact]
        public void happy_path()
        {
            var assertion = new StatusCodeAssertion(304);

            AssertionRunner.Run(assertion, _ => _.StatusCode(304))
                .AssertAll();
        }

        [Fact]
        public void sad_path()
        {
            var assertion = new StatusCodeAssertion(200);

            AssertionRunner.Run(assertion, _ => _.StatusCode(304))
                .SingleMessageShouldBe("Expected a Status Code of 200, but was 304");
        }
    }
}