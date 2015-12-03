using System;
using System.Collections.Generic;
using System.Linq;

namespace Alba.Routing
{
    public class Leaf
    {
        public static ISegment ToParameter(string path, int position)
        {
            if (path == "...")
            {
                return new Spread(position);
            }

            if (path.StartsWith(":"))
            {
                var key = path.Trim(':');
                return new RouteArgument(key, position);
            }

            if (path.StartsWith("{") && path.EndsWith("}"))
            {
                var key = path.TrimStart('{').TrimEnd('}');
                return new RouteArgument(key, position);
            }

            return new Segment(path, position);
        }

        private readonly List<ISegment> _parameters = new List<ISegment>(); 
        private readonly List<ISegment> _segments = new List<ISegment>(); 

        public Leaf(string route, string name = null)
        {
            route = route.TrimStart('/').TrimEnd('/');

            Name = name ?? route;

            var segments = route.Split('/');
            for (int i = 0; i < segments.Length; i++)
            {
                var segment = ToParameter(segments[i], i);
                _segments.Add(segment);
            }

            _parameters.AddRange(_segments.Where(x => !(x is Segment)));

            Route = string.Join("/", _segments.Select(x => x.SegmentPath));


            if (!HasSpread) return;

            var spreads = _parameters.OfType<Spread>().ToArray();
            if (spreads.Count() > 1 || spreads.First().Position != _segments.Count - 1) throw new ArgumentOutOfRangeException(nameof(route), "The spread parameter can only be the last segment in a route");
        }

        public IEnumerable<ISegment> Segments => _segments;


        public bool EndsWithArgument
        {
            get
            {
                if (_segments.LastOrDefault() is RouteArgument)
                {
                    return true;
                }

                if (_segments.LastOrDefault() is Spread && _segments.Count >= 2)
                {
                    return _segments[_segments.Count - 2] is RouteArgument;
                }

                return false;
            }
        }
 
        public string Route { get; }

        public bool HasSpread => _segments.Any(x => x is Spread);

        public string Name { get; }

        public string NodePath
        {
            get
            {
                var segments = _segments.ToArray().Reverse().Skip(1).Reverse().ToArray();

                if (HasSpread && EndsWithArgument)
                {
                    segments = _segments.ToArray().Reverse().Skip(2).Reverse().ToArray();
                }

                return string.Join("/", segments.Select(x => x.CanonicalPath()));
            }
        }

        public string LastSegment => _segments.Count == 0 ? string.Empty : _segments.Last().CanonicalPath();

        public IEnumerable<ISegment> Parameters => _parameters;

        public void SetValues(IDictionary<string, object> env, string[] segments)
        {
            foreach (var parameter in _parameters)
            {
                parameter.SetValues(env, segments);
            }
        }
    }
}