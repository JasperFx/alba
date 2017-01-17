using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class CookieStateTests
    {
        [Fact]
        public void parse_with_one_value()
        {
            var state = CookieState.Parse("key", "1");
            state.Name.ShouldBe("key");
            state.Value.ShouldBe("1");
        }

        [Fact]
        public void parse_with_multiple_values()
        {
            var state = CookieState.Parse("key", "a=1");

            state.Name.ShouldBe("key");
            state.Value.ShouldBeNull();

            state["a"].ShouldBe("1");
        }

        [Fact]
        public void parse_with_multiple_values_2()
        {
            var state = CookieState.Parse("key", "a=1&b=2&c=3");

            state.Name.ShouldBe("key");
            state.Value.ShouldBeNull();

            state["a"].ShouldBe("1");
            state["b"].ShouldBe("2");
            state["c"].ShouldBe("3");
        }

        [Fact]
        public void silly_with_works()
        {
            var state = new CookieState("key").With("a", "1").With("b", "2");

            state["a"].ShouldBe("1");
            state["b"].ShouldBe("2");
        }

        private void roundtrips(string name, string text)
        {
            var state = CookieState.Parse(name, text);
            state.ToString().ShouldBe(name + "=" + text);
        }

        [Fact]
        public void writing()
        {
            roundtrips("a", "1");
            roundtrips("a", "a1=1&a2=2&a3=3");

        }
    }
}