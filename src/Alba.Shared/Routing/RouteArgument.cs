using System.Collections.Generic;

namespace Alba.Routing
{
    public class RouteArgument : ISegment
    {
        public string Key { get; }
        public int Position { get; }
        public string CanonicalPath()
        {
            return "*";
        }

        public string SegmentPath { get; }
        public bool IsParameter => true;

        public RouteArgument(string key, int position)
        {
            Key = key;
            Position = position;

            SegmentPath = ":" + Key;
        }

        public void SetValues(IDictionary<string, object> env, string[] segments)
        {
            var value = segments[Position];
            env.SetRouteData(Key, value);
        }

        protected bool Equals(RouteArgument other)
        {
            return string.Equals(Key, other.Key) && Position == other.Position;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RouteArgument) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Key != null ? Key.GetHashCode() : 0)*397) ^ Position;
            }
        }

        public override string ToString()
        {
            return $"{Key}:{Position}";
        }
    }
}