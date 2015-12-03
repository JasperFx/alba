using System.Collections.Generic;

namespace Alba.Routing
{
    public class RouteTree
    {
        private readonly IDictionary<string, Node> _all = new Dictionary<string, Node>();
        private readonly IDictionary<string, Leaf> _leaves = new Dictionary<string, Leaf>();
        private readonly Node _root;
        private Leaf _home;

        public RouteTree()
        {
            _root = new Node("");
            _all.Add(string.Empty, _root);
        }

        public void AddRoute(string pattern, string name)
        {
            var leaf = new Leaf(pattern, name);
            if (string.IsNullOrEmpty(pattern))
            {
                _home = leaf;
            }

            _leaves.Add(leaf.Route, leaf);
            var node = getNode(leaf.NodePath);
            node.AddLeaf(leaf);

        }

        private Node getNode(string nodePath)
        {
            if (_all.ContainsKey(nodePath)) return _all[nodePath];

            var node = new Node(nodePath);
            _all.Add(node.Route, node);

            if (node.ParentRoute != null)
            {
                var parent = getNode(node.ParentRoute);
                parent.AddChild(node);
            }



            return node;
        }

        public Leaf Select(string route)
        {
            if (string.IsNullOrEmpty(route.Trim())) return _home;

            var segments = ToSegments(route);

            return Select(segments);
        }

        public Leaf Select(string[] segments)
        {
            return _root.Select(segments, 0);
        }

        public static string[] ToSegments(string route)
        {
            return route.Trim().TrimStart('/').TrimEnd('/').Split('/');
        }
    }
}