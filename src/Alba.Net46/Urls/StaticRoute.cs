using Alba.Routing;

namespace Alba.Urls
{
    public class StaticRoute : IRoute
    {
        public Leaf Leaf { get; }
        public string HttpMethod { get; }

        public bool HasParameters => false;

        public StaticRoute(Leaf leaf, string httpMethod)
        {
            Leaf = leaf;
            HttpMethod = httpMethod;
        }

        public void Register(IUrlGraph graph)
        {
            graph.Register(Leaf.Name, this);
        }
    }
}