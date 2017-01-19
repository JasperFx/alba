using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace AlbaRouter
{
    public class Spread : ISegment
    {
        public int Position { get; }
        public string CanonicalPath()
        {
            return string.Empty;
        }

        public string SegmentPath { get; } = "...";
        public bool IsParameter => true;
        public string SegmentFromModel(object model)
        {
            throw new NotSupportedException();
        }

        public Spread(int position)
        {
            Position = position;
        }

        public void SetValues(HttpContext routeData, string[] segments)
        {
            var spreadData = getSpreadData(segments);
            routeData.SetSpreadData(spreadData);
        }

        private string[] getSpreadData(string[] segments)
        {
            if (segments.Length == 0) return new string[0];

            if (Position == 0) return segments;

            if (Position > (segments.Length - 1)) return new string[0];

            return segments.Skip(Position).ToArray();
        }

        public string ReadRouteDataFromMethodArguments(List<object> arguments)
        {
            throw new NotSupportedException();
        }

        public string SegmentFromParameters(IDictionary<string, object> parameters)
        {
            throw new NotSupportedException();
        }

        public override string ToString()
        {
            return $"spread:{Position}";
        }

        protected bool Equals(Spread other)
        {
            return Position == other.Position;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Spread) obj);
        }

        public override int GetHashCode()
        {
            return Position;
        }
    }
}