using Alba.Routing;

namespace Alba.Urls
{
    public class StaticRoute : IRoute
    {
        public Route Route { get; }

        public StaticRoute(Route route)
        {
            Route = route;
        }

        public void Register(IUrlGraph graph)
        {
            graph.Register(Route.Name, this);
        }
    }
}