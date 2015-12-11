using System.Collections.Generic;
using System.Linq;

namespace Alba.Routing
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

        public Spread(int position)
        {
            Position = position;
        }

        public void SetValues(IDictionary<string, object> routeData, string[] segments)
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