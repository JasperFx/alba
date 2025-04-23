using Alba.Assertions;

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
                .SingleMessageShouldBe("Expected status code 200, but was 304");
        }
    }
}