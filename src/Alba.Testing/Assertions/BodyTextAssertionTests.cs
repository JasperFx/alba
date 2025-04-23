using Alba.Assertions;
using Shouldly;

namespace Alba.Testing.Assertions
{
    public class BodyTextAssertionTests
    {
        [Fact]
        public void happy_path()
        {
            AssertionRunner.Run(new BodyTextAssertion("Hey!"), env => env.Response.Write("Hey!"))
                .AssertAll();
        }

        [Fact]
        public void sad_path()
        {
            var assertion = new BodyTextAssertion("Hey!");
            AssertionRunner.Run(assertion, env => env.Response.Write("Hey! You!"))
                .Messages.Single().ShouldContain("Expected the content to be 'Hey!'");
        }
    }
}