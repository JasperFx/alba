using System.Collections.Generic;

namespace Alba.Routing
{
    public interface ISegment
    {
        int Position { get; }
        string CanonicalPath();

        string SegmentPath { get; }
        bool IsParameter { get; }
        string SegmentFromModel(object model);

        string ReadRouteDataFromMethodArguments(List<object> arguments);
        string SegmentFromParameters(IDictionary<string, object> parameters);
    }
}