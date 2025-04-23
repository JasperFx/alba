using Alba.Assertions;

namespace Alba.Testing.Assertions
{
    public class BodyDoesNotContainAssertionTests
    {
        [Fact]
        public void happy_path()
        {
            AssertionRunner.Run(new BodyDoesNotContainAssertion("Hey!"), env => env.Response.Write("You!"))
                .AssertAll();
        }

        [Fact]
        public void sad_path()
        {
            var assertion = new BodyDoesNotContainAssertion("Hey!");
            AssertionRunner.Run(assertion, env => env.Response.Write("Hey! You!"))
                .SingleMessageShouldBe("Text 'Hey!' should not be found in the response body");
        }
    }
}