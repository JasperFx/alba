using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Baseline.Conversion;

namespace Alba.Routing
{
    public class UrlGraph : IUrlRegistry
    {
        public static readonly Conversions Conversions = new Conversions();

        private readonly LightweightCache<Type, List<Route>> _routesByInputModel
            = new LightweightCache<Type, List<Route>>(_ => new List<Route>());

        private readonly LightweightCache<Type, HandlerMethods> _routesPerHandler
            = new LightweightCache<Type, HandlerMethods>(type => new HandlerMethods(type));

        private readonly LightweightCache<string, Route> _routesPerName
            = new LightweightCache<string, Route>();


        public void RegisterByHandler(Type handlerType, MethodInfo method, Route route)
        {
            throw new NotImplementedException();
        }

        public void RegisterByInput(Type inputModel, Route route)
        {
            throw new NotImplementedException();
        }

        public void Register(string name, Route route)
        {
            throw new NotImplementedException();
        }

        public string UrlFor(object model, string httpMethod = null)
        {
            throw new NotImplementedException();
        }

        public string UrlFor<T>(string httpMethod = null) where T : class
        {
            // look for input model first, but blow up if it requires any parameters
            // look next by handler type. If only one method and no parameters, use that.
            // if more than one, throw
            throw new NotImplementedException();
        }

        public string UrlFor(Type handlerType, MethodInfo method = null, string httpMethod = null)
        {
            // Do above if method is null, otherwise go to method
            throw new NotImplementedException();
        }

        public string UrlFor<THandler>(Expression<Action<THandler>> expression, string httpMethod = null)
        {
            // find by method
            throw new NotImplementedException();
        }

        public string UrlFor(string routeName, IDictionary<string, object> parameters = null)
        {
            // has to be a static route, or blow up
            throw new NotImplementedException();
        }
    }
}