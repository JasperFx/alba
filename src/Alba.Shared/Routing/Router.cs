using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;

namespace Alba.Routing
{
    public class Router
    {
        private readonly IDictionary<string, RouteTree> _trees = new Dictionary<string, RouteTree>(); 

        public Router()
        {
            HttpVerbs.All.Each(x => _trees.Add(x, new RouteTree()));
        }

        public void Add(string method, string pattern, Func<IDictionary<string, object>, Task> appfunc)
        {
            var route = new Route(pattern, method, appfunc);

            _trees[method.ToUpperInvariant()].AddRoute(route);
        }

        // TODO -- dunno that this needs to be done by verb. Reconsider
        public void AddNotFoundHandler(string method, Func<IDictionary<string, object>, Task> appfunc)
        {
            _trees[method.ToUpperInvariant()].NotFound = appfunc;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            var method = env.HttpMethod();
            var routeTree = _trees[method];

            var segments = RouteTree.ToSegments(env.RelativeUrlWithoutQueryString());
            var leaf = routeTree.Select(segments);

            if (leaf == null) return routeTree.NotFound(env);

            env.StatusCode(200);

            leaf.SetValues(env, segments);
            return leaf.AppFunc(env);
        }
    }
}