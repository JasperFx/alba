using Shouldly;
using Xunit;

namespace AlbaRouter.Testing
{
    public class SegmentTests
    {
        [Fact]
        public void canonical_path_is_just_the_segment()
        {
            new Segment("foo", 2).CanonicalPath().ShouldBe("foo");
        } 
    }
}