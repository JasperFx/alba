using System.Linq;
using Alba.Assertions;
using Shouldly;
using Xunit;

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
                .Messages.Single().ShouldBe("Expected the content to be 'Hey!', but was 'Hey! You!'");
        }
    }
}