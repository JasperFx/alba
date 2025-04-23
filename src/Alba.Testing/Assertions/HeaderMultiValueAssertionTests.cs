using Alba.Assertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Alba.Testing.Assertions
{
    public class HeaderMultiValueAssertionTests
    {
        [Fact]
        public void happy_path()
        {
            var headers = new[] {"one", "two"};
            var assertion = new HeaderMultiValueAssertion("foo", headers);
            AssertionRunner.Run(
                    assertion,
                    x => x.Response.Headers.Append("foo", new StringValues(headers)))
                .AssertAll();
        }

        [Fact]
        public void happy_path_single_value()
        {
            var headers = new[] {"one"};
            var assertion = new HeaderMultiValueAssertion("foo", headers);
            AssertionRunner.Run(
                    assertion,
                    x => x.Response.Headers.Append("foo", new StringValues(headers)))
                .AssertAll();
        }

        [Fact]
        public void sad_path_no_value()
        {
            var expected = new[] {"one", "two"};
            var assertion = new HeaderMultiValueAssertion("foo", expected);
            AssertionRunner.Run(assertion, x => { })
                .SingleMessageShouldBe("Expected header values of 'foo'='one', 'two', but no values were found on the response.");
        }

        [Fact]
        public void sad_path_missing_one()
        {
            var expected = new[] {"one", "two"};
            var actual = new[] {"one"};
            var assertion = new HeaderMultiValueAssertion("foo", expected);
            AssertionRunner.Run(
                    assertion,
                    x => x.Response.Headers.Append("foo", new StringValues(actual)))
                .SingleMessageShouldBe("Expected header values of 'foo'='one', 'two', but the actual values were 'one'.");
        }

        [Fact]
        public void sad_path_wrong_value()
        {
            var expected = new[] {"one", "two"};
            var actual = new[] {"three", "four"};
            var assertion = new HeaderMultiValueAssertion("foo", expected);
            AssertionRunner.Run(
                    assertion,
                    x => x.Response.Headers.Append("foo", new StringValues(actual)))
                .SingleMessageShouldBe("Expected header values of 'foo'='one', 'two', but the actual values were 'three', 'four'.");
        }
    }
}
