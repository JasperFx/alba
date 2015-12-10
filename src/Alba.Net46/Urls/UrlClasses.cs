using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using Alba.Routing;
using Alba.Util;

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


    public interface IRouteWithInputModel : IRoute
    {
        IDictionary<string, string> ToParameters(object model);

        void ApplyValues(object input, IDictionary<string, string> rawValues);
    }

    public class RouteWithInputModel<T> : IRouteWithInputModel
    {
        private readonly IList<IParameter> _parameters = new List<IParameter>(); 

        public Leaf Leaf { get; }
        public string HttpMethod { get; }

        public RouteWithInputModel(Leaf leaf, string httpMethod)
        {
            Leaf = leaf;
            HttpMethod = httpMethod;
        }

        public void Register(IUrlGraph graph)
        {
            throw new NotImplementedException();
        }

        public bool HasParameters { get; }
        public IDictionary<string, string> ToParameters(object model)
        {
            throw new NotImplementedException();
        }

        public void ApplyValues(object input, IDictionary<string, string> rawValues)
        {
            throw new NotImplementedException();
        }

        public void AddFieldParam(string name, string key = null)
        {
            throw new NotImplementedException();
        }

        public void AddPropertyParam(string name, string key = null)
        {
            throw new NotImplementedException();
        }

        public void AddFieldParam(FieldInfo field, string key = null)
        {
            throw new NotImplementedException();
        }

        public void AddPropertyParam(PropertyInfo property, string key = null)
        {
            throw new NotImplementedException();
        }



        internal interface IParameter
        {
            string Read(object input);
            string Key { get; }
            void Write(object input, string raw);
        }

        public class FieldInfoParameter : IParameter
        {
            public FieldInfoParameter(FieldInfo field, string key = null)
            {
                Key = key ?? field.Name;
            }

            public string Read(object input)
            {
                throw new NotImplementedException();
            }

            public string Key { get; }
            public void Write(object input, string raw)
            {
                throw new NotImplementedException();
            }
        }

        public class PropertyInfoParameter : IParameter
        {
            public PropertyInfoParameter(PropertyInfo property, string key = null)
            {
                Key = key ?? property.Name;
            }

            public string Read(object input)
            {
                throw new NotImplementedException();
            }

            public string Key { get; }
            public void Write(object input, string raw)
            {
                throw new NotImplementedException();
            }
        }
    }
}