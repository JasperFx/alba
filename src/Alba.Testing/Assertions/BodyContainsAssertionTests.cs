using Alba.Assertions;

namespace Alba.Testing.Assertions
{
    public class BodyContainsAssertionTests
    {
        [Fact]
        public void happy_path()
        {
            AssertionRunner.Run(new BodyContainsAssertion("Hey!"), env => env.Response.Write("Hey! You!"))
                .AssertAll();
        }

        [Fact]
        public void sad_path()
        {
            var assertion = new BodyContainsAssertion("Hey!");
            AssertionRunner.Run(assertion, env => env.Response.Write("Not the droids you are looking for"))
                .SingleMessageShouldBe("Expected text 'Hey!' was not found in the response body");
        }
    }
}