using System.Collections.Generic;
using System.Linq;

namespace AlbaRouter
{
    public interface INode
    {
        string Route { get; }
        Route SpreadRoute { get; set; }
        Route Select(string[] segments, int position);
    }

    public class Node : INode
    {
        public Node(string route)
        {
            Route = route;

            ParentRoute = string.IsNullOrEmpty(route) 
                ? null 
                : string.Join("/", route.Split('/').Reverse().Skip(1).Reverse().ToArray());
        }

        // Use this to "know" where to put this in the tree
        // could be blank
        public string ParentRoute { get; }

        public INode Parent { get; private set; }

        public string Route { get; }

        public Route SpreadRoute { get; set; }
        public IDictionary<string, Route> NamedLeaves { get; } = new Dictionary<string, Route>(); 
        public IDictionary<string, INode> NamedNodes { get; } = new Dictionary<string, INode>(); 

        public IList<INode> ArgNodes { get; } = new List<INode>();

        public Route Select(string[] segments, int position)
        {
            var hasMore = position < segments.Length - 1;
            var current = segments[position];

            if (!hasMore)
            {
                if (NamedLeaves.ContainsKey(current))
                {
                    return NamedLeaves[current];
                }

                if (NamedNodes.ContainsKey(current))
                {
                    var leaf = NamedNodes[current].SpreadRoute;
                    if (leaf != null) return leaf;
                }

                if (ArgRoute != null) return ArgRoute;
                if (SpreadRoute != null) return SpreadRoute;
            }
            else
            {
                if (NamedNodes.ContainsKey(current))
                {
                    var leaf = NamedNodes[current].Select(segments, position + 1);
                    if (leaf != null) return leaf;
                }

                foreach (var node in ArgNodes)
                {
                    var leaf = node.Select(segments, position + 1);
                    if (leaf != null) return leaf;
                }
            }


            

            return SpreadRoute;
            
        }

        public void AddChild(Node child)
        {
            child.Parent = this;

            var lastSegment = child.LastSegment();
            if (lastSegment == "*")
            {
                ArgNodes.Add(child);
            }
            else
            {
                NamedNodes.Add(lastSegment, child);
            }
        }

        private string LastSegment()
        {
            return Route.Split('/').Last();
        }

        public void AddLeaf(Route route)
        {
            if (route.HasSpread)
            {
                SpreadRoute = route;
            }
            else if (route.EndsWithArgument)
            {
                ArgRoute = route;
            }
            else
            {
                NamedLeaves.Add(route.LastSegment, route);
            }
        }

        public Route ArgRoute { get; private set; }
    }
}