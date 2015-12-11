using System.Collections.Generic;

namespace Alba.Routing
{
    public interface ISegment
    {
        int Position { get; }
        string CanonicalPath();

        string SegmentPath { get; }
        bool IsParameter { get; }
    }
}