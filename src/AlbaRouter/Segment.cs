using System.Collections.Generic;

namespace AlbaRouter
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
        public string SegmentFromModel(object model)
        {
            return Path;
        }

        public string ReadRouteDataFromMethodArguments(List<object> arguments)
        {
            return Path;
        }

        public string SegmentFromParameters(IDictionary<string, object> parameters)
        {
            return Path;
        }

        public Segment(string path, int position)
        {
            Path = path;
            Position = position;
            SegmentPath = path;
        }



    }
}