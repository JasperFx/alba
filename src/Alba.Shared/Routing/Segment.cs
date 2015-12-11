using System.Collections.Generic;

namespace Alba.Routing
{
    public class Segment : ISegment
    {
        public string Path { get; }
        public int Position { get; }
        public string CanonicalPath()
        {
            return Path;
        }

        public string SegmentPath { get; }
        public bool IsParameter => false;

        public Segment(string path, int position)
        {
            Path = path;
            Position = position;
            SegmentPath = path;
        }

        public void SetValues(IDictionary<string, object> env, string[] segments)
        {
            // nullo
        }


    }
}