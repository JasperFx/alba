using Alba.Scenarios.Assertions;
using Alba.Testing.Scenarios.Assertions;
using Xunit;

namespace Alba.Testing.Scenarios
{
    public class BodyContainsAssertionTests
    {
        [Fact]
        public void happy_path()
        {
            AssertionRunner.Run(new BodyContainsAssertion("Hey!"), env => env.Write("Hey! You!"))
                .AssertValid();
        }

        [Fact]
        public void sad_path()
        {
            var assertion = new BodyContainsAssertion("Hey!");
            AssertionRunner.Run(assertion, env => env.Write("Not the droids you are looking for"))
                .SingleMessageShouldBe("Expected text 'Hey!' was not found in the response body");
        }
    }
}