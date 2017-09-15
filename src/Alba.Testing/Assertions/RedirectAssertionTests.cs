using Alba.Assertions;
using Alba.Stubs;
using Baseline;
using Xunit;

namespace Alba.Testing.Assertions
{
    public class RedirectAssertionTests
    {
        [Fact]
        public void happy_path()
        {
            var assertion = new RedirectAssertion("/to", false);
            AssertionRunner
                .Run(assertion, x => x.Response.As<StubHttpResponse>().Redirect("/to"))
                .AssertAll();
        }

        [Fact]
        public void sad_path_no_value()
        {
            var assertion = new RedirectAssertion("/to", false);
            AssertionRunner
                .Run(assertion, x => { })
                .SingleMessageShouldBe("Expected to be redirected to '/to' but was ''.");
        }

        [Fact]
        public void sad_path_wrong_value()
        {
            var assertion = new RedirectAssertion("/to", false);
            AssertionRunner
                .Run(assertion, x => x.Response.As<StubHttpResponse>().Redirect("/wrong"))
                .SingleMessageShouldBe("Expected to be redirected to '/to' but was '/wrong'.");
        }

        [Fact]
        public void happy_path_permanent()
        {
            var assertion = new RedirectAssertion("/to", true);
            AssertionRunner
                .Run(assertion, x => x.Response.As<StubHttpResponse>().Redirect("/to", true))
                .AssertAll();
        }

        [Fact]
        public void sad_path_permanent_wrong_value()
        {
            var assertion = new RedirectAssertion("/to", false);
            AssertionRunner
                .Run(assertion, x => x.Response.As<StubHttpResponse>().Redirect("/to", true))
                .SingleMessageShouldBe("Expected permanent redirect to be 'False' but it was not.");
        }
    }
}
