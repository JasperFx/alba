using Alba.Scenarios.Assertions;
using Alba.Testing.Scenarios.Assertions;
using Xunit;

namespace Alba.Testing.Scenarios
{
    public class BodyDoesNotContainAssertionTests
    {
        [Fact]
        public void happy_path()
        {
            AssertionRunner.Run(new BodyDoesNotContainAssertion("Hey!"), env => env.Write("You!"))
                .AssertAll();
        }

        [Fact]
        public void sad_path()
        {
            var assertion = new BodyDoesNotContainAssertion("Hey!");
            AssertionRunner.Run(assertion, env => env.Write("Hey! You!"))
                .SingleMessageShouldBe("Text 'Hey!' should not be found in the response body");
        }
    }
}