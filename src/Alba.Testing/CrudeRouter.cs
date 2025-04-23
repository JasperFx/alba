using System.Linq.Expressions;
using System.Reflection;
using JasperFx.Core;
using JasperFx.Core.Reflection;
using Microsoft.AspNetCore.Http;

namespace Alba.Testing
{
    public class CrudeRouter 
    {
        public readonly LightweightCache<string, RequestDelegate> Handlers = new LightweightCache<string, RequestDelegate>(
            path => throw new NotImplementedException());


        public Task Invoke(HttpContext context)
        {
            return Handlers[context.Request.Path](context);
        }
        
        internal class Route
        {
            public Type HandlerType;
            public MethodInfo Method;
            public string Url;
            public string HttpMethod;
        }

        private readonly IList<Route> _routes = new List<Route>();

        public void RegisterRoute<T>(Expression<Action<T>> expression, string method, string route)
        {
            _routes.Add(new Route
            {
                HttpMethod = method,
                HandlerType = typeof(T),
                Url = route,
                Method = ReflectionHelper.GetMethod(expression)
            });
        }
        
        public string UrlFor<T>(Expression<Action<T>> expression, string httpMethod)
        {
            var method = ReflectionHelper.GetMethod(expression);

            var route =
                _routes.Single(x => x.HttpMethod == httpMethod && x.HandlerType == typeof(T) && x.Method.Name == method.Name);

            return route.Url;
        }

        public string UrlFor<T>(string method)
        {
            throw new NotImplementedException();
        }

        public string UrlFor<T>(T input, string httpMethod)
        {
            return null;
        }

    }
}