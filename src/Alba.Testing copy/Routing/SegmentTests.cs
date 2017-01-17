using Shouldly;
using Xunit;

namespace Alba.Testing.Routing
{
    public class SegmentTests
    {
        [Fact]
        public void canonical_path_is_just_the_segment()
        {
            new Alba.Routing.Segment("foo", 2).CanonicalPath().ShouldBe("foo");
        } 
    }
}