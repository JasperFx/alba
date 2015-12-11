using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;


namespace Alba.Routing
{
    public class Route
    {
        /// <summary>
        /// This is only for testing purposes
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Route For(string url, string httpMethod)
        {
            return new Route(url, httpMethod ?? HttpVerbs.GET, env => Task.CompletedTask);
        }

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


        public Route(string pattern, string httpMethod, AppFunc appFunc)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            if (appFunc == null) throw new ArgumentNullException(nameof(appFunc));

            pattern = pattern.TrimStart('/').TrimEnd('/');

            Name = pattern;
            HttpMethod = httpMethod;
            AppFunc = appFunc;

            var segments = pattern.Split('/');
            for (int i = 0; i < segments.Length; i++)
            {
                var segment = ToParameter(segments[i], i);
                _segments.Add(segment);
            }

            _parameters.AddRange(_segments.Where(x => !(x is Segment)));

            Pattern = string.Join("/", _segments.Select(x => x.SegmentPath));


            if (!HasSpread) return;

            var spreads = _parameters.OfType<Spread>().ToArray();
            if (spreads.Count() > 1 || spreads.First().Position != _segments.Count - 1) throw new ArgumentOutOfRangeException(nameof(pattern), "The spread parameter can only be the last segment in a route");
        }

        public Route(ISegment[] segments, string httpVerb, AppFunc appfunc)
        {
            _segments.AddRange(segments);
            _parameters.AddRange(segments.Where(x => x.IsParameter));
            HttpMethod = httpVerb;
            AppFunc = appfunc;

            Pattern = _segments.Select(x => x.SegmentPath).Join("/");
            Name = Pattern;
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

        public bool HasParameters => _parameters.Any();

        public string Pattern { get; }

        public bool HasSpread => _segments.Any(x => x is Spread);

        public string Name { get; set; }
        public string HttpMethod { get; }
        public AppFunc AppFunc { get; }

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