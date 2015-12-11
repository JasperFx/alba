using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alba.Routing;
using Alba.Testing.Routing;
using StoryTeller;
using StoryTeller.Grammars.Tables;

namespace Alba.Testing.Fixtures
{
    public class LeafFixture : Fixture
    {
        [ExposeAsTable("Route Creation and Parsing")]
        public void CreateLeaf(string Route, out string NodePath, [Default("false")]out bool HasSpread, [Default("NONE")] out ParameterExpectation Parameters)
        {
            var leaf = new Route(Route, HttpVerbs.GET, env => Task.CompletedTask);
            NodePath = leaf.NodePath;
            HasSpread = leaf.HasSpread;

            Parameters = new ParameterExpectation(leaf);
        }
    }

    public class ParameterExpectation
    {
        private readonly List<ISegment> _segments = new List<ISegment>(); 

        public ParameterExpectation(string text)
        {
            if (text != "NONE")
            {
                var parts = text.Split(';');

                foreach (var part in parts)
                {
                    var vals = part.Split(':');
                    var key = vals[0].Trim();
                    var position = int.Parse(vals[1]);

                    if (key == "spread")
                    {
                        _segments.Add(new Spread(position));
                    }
                    else
                    {
                        _segments.Add(new RouteArgument(key, position));
                    }
                }
            }


        }

        public ParameterExpectation(Route route)
        {
            _segments.AddRange(route.Parameters);
        }

        public override string ToString()
        {
            if (!_segments.Any())
            {
                return "NONE";
            }

            var segments = _segments.OrderBy(x => x.Position).Select(x => x.ToString()).ToArray();
            return string.Join("; ", segments);
        }

        protected bool Equals(ParameterExpectation other)
        {
            if (_segments.Count == 0) return other._segments.Count == 0;

            return _segments.OrderBy(x => x.Position).SequenceEqual(other._segments.OrderBy(x => x.Position));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ParameterExpectation) obj);
        }

        public override int GetHashCode()
        {
            return (_segments != null ? _segments.GetHashCode() : 0);
        }
    }
}