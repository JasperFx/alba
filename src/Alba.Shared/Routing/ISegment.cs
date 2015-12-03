using System.Collections.Generic;

namespace Alba.Routing
{
    public interface ISegment
    {
        void SetValues(IDictionary<string, object> env, string[] segments);
        int Position { get; }
        string CanonicalPath();

        string SegmentPath { get; }
    }
}