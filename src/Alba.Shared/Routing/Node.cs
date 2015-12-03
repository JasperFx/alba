using System.Collections.Generic;
using System.Linq;

namespace Alba.Routing
{
    public interface INode
    {
        string Route { get; }
        Leaf SpreadLeaf { get; set; }
        Leaf Select(string[] segments, int position);
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

        public Leaf SpreadLeaf { get; set; }
        public IDictionary<string, Leaf> NamedLeaves { get; } = new Dictionary<string, Leaf>(); 
        public IDictionary<string, INode> NamedNodes { get; } = new Dictionary<string, INode>(); 

        public IList<INode> ArgNodes { get; } = new List<INode>();

        public Leaf Select(string[] segments, int position)
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
                    var leaf = NamedNodes[current].SpreadLeaf;
                    if (leaf != null) return leaf;
                }

                if (ArgLeaf != null) return ArgLeaf;
                if (SpreadLeaf != null) return SpreadLeaf;
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


            

            return SpreadLeaf;
            
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

        public void AddLeaf(Leaf leaf)
        {
            if (leaf.HasSpread)
            {
                SpreadLeaf = leaf;
            }
            else if (leaf.EndsWithArgument)
            {
                ArgLeaf = leaf;
            }
            else
            {
                NamedLeaves.Add(leaf.LastSegment, leaf);
            }
        }

        public Leaf ArgLeaf { get; private set; }
    }
}