using System;
using System.Collections.Generic;
using Baseline.Conversion;

namespace Alba.Routing
{
    public class RouteArgument : ISegment
    {
        public static readonly Conversions Conversions = new Conversions();

        private Func<string, object> _converter = x => x; 

        private Type _argType;
        public string Key { get; }
        public int Position { get; }
        public string CanonicalPath()
        {
            return "*";
        }

        public string SegmentPath { get; }
        public bool IsParameter => true;

        public RouteArgument(string key, int position, Type argType = null)
        {
            ArgType = argType ?? typeof (string);
            Key = key;
            Position = position;

            SegmentPath = ":" + Key;
        }

        public Type ArgType
        {
            get { return _argType; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _argType = value;

                _converter = Conversions.FindConverter(ArgType);
            }
        }

        public void SetValues(IDictionary<string, object> env, string[] segments)
        {
            var raw = segments[Position];
            env.SetRouteData(Key, _converter(raw));
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