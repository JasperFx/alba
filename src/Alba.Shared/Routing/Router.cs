using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Alba.Routing
{
    public class Router
    {
        private readonly IDictionary<string, RouteTree> _trees = new Dictionary<string, RouteTree>(); 

        public Router()
        {
            _trees.Add("GET", new RouteTree());
            _trees.Add("POST", new RouteTree());
            _trees.Add("PUT", new RouteTree());
            _trees.Add("DELETE", new RouteTree());
            _trees.Add("HEAD", new RouteTree());
        }

        public void Add(string method, string pattern, Func<IDictionary<string, object>, Task> appfunc)
        {
            _trees[method.ToUpperInvariant()].AddRoute(pattern, appfunc);
        }

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