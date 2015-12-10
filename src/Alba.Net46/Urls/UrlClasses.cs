using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Alba.Routing;
using Baseline;
using Baseline.Conversion;

namespace Alba.Urls
{
    // getting a string url
    public interface IUrlRegistry
    {
        string UrlFor(object model, string httpMethod = null);

        string UrlFor<T>(string httpMethod = null) where T : class;

        string UrlFor(Type handlerType, MethodInfo method = null, string httpMethod = null);

        string UrlFor<THandler>(Expression<Action<THandler>> expression, string httpMethod = null);

        string UrlFor(string routeName);
    }

    /* JS route:
    {name: "", method: "", url: "", parameters: []}



    */

    internal class HandlerMethods
    {
        private readonly LightweightCache<string, IRoute> _routesByMethod
            = new LightweightCache<string, IRoute>();

        public Type HandlerType { get; }

        public HandlerMethods(Type handlerType)
        {
            HandlerType = handlerType;
        }


    }

    public class UrlGraph : IUrlRegistry, IUrlGraph
    {
        public static readonly Conversions Conversions = new Conversions();

        private readonly LightweightCache<Type, List<IRouteWithInputModel>> _routesByInputModel
            = new LightweightCache<Type, List<IRouteWithInputModel>>(_ => new List<IRouteWithInputModel>());

        private readonly LightweightCache<Type, HandlerMethods> _routesPerHandler
            = new LightweightCache<Type, HandlerMethods>(type => new HandlerMethods(type));

        private readonly LightweightCache<string, IRoute> _routesPerName
            = new LightweightCache<string, IRoute>();


        public void RegisterByHandler(Type handlerType, MethodInfo method, IRoute route)
        {
            throw new NotImplementedException();
        }

        public void RegisterByInput(Type inputModel, IRouteWithInputModel route)
        {
            throw new NotImplementedException();
        }

        public void Register(string name, IRoute route)
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

        public string UrlFor(string routeName)
        {
            // has to be a static route, or blow up
            throw new NotImplementedException();
        }
    }

    public interface IRoute
    {
        Leaf Leaf { get; }

        string HttpMethod { get; }

        void Register(IUrlGraph graph);

        bool HasParameters { get; }
    }
}