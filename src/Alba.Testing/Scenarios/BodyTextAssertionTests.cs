using System.Linq;
using Alba.Scenarios.Assertions;
using Alba.Testing.Scenarios.Assertions;
using Shouldly;
using Xunit;

namespace Alba.Testing.Scenarios
{
    public class BodyTextAssertionTests
    {
        [Fact]
        public void happy_path()
        {
            AssertionRunner.Run(new BodyTextAssertion("Hey!"), env => env.Write("Hey!"))
                .AssertAll();
        }

        [Fact]
        public void sad_path()
        {
            var assertion = new BodyTextAssertion("Hey!");
            AssertionRunner.Run(assertion, env => env.Write("Hey! You!"))
                .Messages.Single().ShouldContain("The contents should have been:");
        }
    }
}