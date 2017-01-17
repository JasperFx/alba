using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class SegmentTests
    {
        [Fact]
        public void parse_with_unquoted_value()
        {
            var segment = new CookieSegment("a=1");

            segment.Key.ShouldBe("a");
            segment.Value.ShouldBe("1");
        }

        [Fact]
        public void parse_with_no_value_uses_defaults()
        {
            var segment = new CookieSegment("a");
            segment.Key.ShouldBe("a");
            segment.Value.ShouldBeNull();
        }

        [Fact]
        public void parse_with_quoted_value()
        {
            var segment = new CookieSegment("a=\"how are you doing\"");

            segment.Key.ShouldBe("a");
            segment.Value.ShouldBe("how are you doing");
        }

        [Fact]
        public void only_splits_on_first_equals()
        {
            var segment = new CookieSegment("TestCookie=a1=b1&a2=b2");
            segment.Key.ShouldBe("TestCookie");

            segment.Value.ShouldBe("a1=b1&a2=b2");
        }
    }
}