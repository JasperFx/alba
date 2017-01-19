using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;
using Microsoft.AspNetCore.Http;

namespace AlbaRouter
{
    public class Router
    {
        private readonly IDictionary<string, RouteTree> _trees = new Dictionary<string, RouteTree>();

        public Router()
        {
            HttpVerbs.All.Each(x => _trees.Add(x, new RouteTree()));
        }

        public void Add(string method, string pattern,RequestDelegate appfunc)
        {
            var route = new Route(pattern, method, appfunc);

            Add(route);

            Urls.Register(route);
        }

        public UrlGraph Urls { get; } = new UrlGraph();

        public void Add(Route route)
        {
            _trees[route.HttpMethod.ToUpperInvariant()].AddRoute(route);
        }

        // TODO -- dunno that this needs to be done by verb. Reconsider
        public void AddNotFoundHandler(string method, RequestDelegate handler)
        {
            _trees[method.ToUpperInvariant()].NotFound = handler;
        }

        public Task Invoke(HttpContext context)
        {
            var method = context.Request.Method;
            var routeTree = _trees[method];

            var segments = RouteTree.ToSegments(context.Request.Path);
            var leaf = routeTree.Select(segments);

            if (leaf == null) return routeTree.NotFound(context);

            context.Response.StatusCode = 200;

            leaf.SetValues(context, segments);

            return leaf.Invoker(context);
        }

        public RouteAdder Get => new RouteAdder(HttpVerbs.GET, this);
        public RouteAdder Post => new RouteAdder(HttpVerbs.POST, this);
        public RouteAdder Put => new RouteAdder(HttpVerbs.PUT, this);
        public RouteAdder Delete => new RouteAdder(HttpVerbs.DELETE, this);
        public RouteAdder Head => new RouteAdder(HttpVerbs.HEAD, this);
        public RouteAdder Options => new RouteAdder(HttpVerbs.OPTIONS, this);
        public RouteAdder Patch => new RouteAdder(HttpVerbs.PATCH, this);
    }

    public class RouteAdder
    {
        private readonly string _httpVerb;
        private readonly Router _parent;

        public RouteAdder(string httpVerb, Router parent)
        {
            _httpVerb = httpVerb;
            _parent = parent;
        }

        public RequestDelegate this[string pattern]
        {
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                _parent.Add(_httpVerb, pattern, value);
            }
        }
    }
}