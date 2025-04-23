using Alba.Assertions;
using Shouldly;

namespace Alba.Testing.Assertions
{
    public class RedirectAssertionTests
    {
        [Fact]
        public void happy_path()
        {
            var assertion = new RedirectAssertion("/to", false);
            AssertionRunner
                .Run(assertion, x => x.Response.Redirect("/to"))
                .AssertAll();
        }

        [Fact]
        public void sad_path_no_value()
        {
            var assertion = new RedirectAssertion("/to", false);
            AssertionRunner
                .Run(assertion, x => { })
                .Messages.FirstOrDefault()
                .ShouldBe("Expected to be redirected to '/to' but was ''.");
        }

        [Fact]
        public void sad_path_wrong_value()
        {
            var assertion = new RedirectAssertion("/to", false);
            AssertionRunner
                .Run(assertion, x => x.Response.Redirect("/wrong"))
                .SingleMessageShouldBe("Expected to be redirected to '/to' but was '/wrong'.");
        }

        [Fact]
        public void happy_path_permanent()
        {
            var assertion = new RedirectAssertion("/to", true);
            AssertionRunner
                .Run(assertion, x => x.Response.Redirect("/to", true))
                .AssertAll();
        }

        [Fact]
        public void sad_path_permanent_wrong_value()
        {
            var assertion = new RedirectAssertion("/to", false);
            AssertionRunner
                .Run(assertion, x => x.Response.Redirect("/to", true))
                .SingleMessageShouldBe("Expected status code 302, but was 301");
        }
    }
}
