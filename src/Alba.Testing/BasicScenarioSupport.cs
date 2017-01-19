using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Alba.Stubs;
using Baseline;
using Baseline.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StructureMap;

namespace Alba.Testing
{
    public class BasicScenarioSupport : ISystemUnderTest
    {
        public readonly Container Container;

        private readonly JsonSerializer _serializer = new JsonSerializer
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };

        public BasicScenarioSupport()
        {
            var registry = new Registry();
            registry.Populate(new ServiceDescriptor[0]);

            Container = new Container(registry);
        }

        public HttpContext CreateContext()
        {
            return new StubHttpContext(Features, Services);
        }

        public IFeatureCollection Features { get; } = null;
        public IServiceProvider Services => new StructureMapServiceProvider(Container);
        public RequestDelegate Invoker => Invoke;


        public readonly LightweightCache<string, RequestDelegate> Handlers = new LightweightCache<string, RequestDelegate>(
            path =>
            {
                throw new NotImplementedException();
            });



        public Task Invoke(HttpContext context)
        {
            return Handlers[context.Request.Path](context);
        }

        public Task BeforeEach(HttpContext context)
        {
            return Task.CompletedTask;
        }

        public Task AfterEach(HttpContext context)
        {
            return Task.CompletedTask;
        }


        public string ToJson(object document)
        {
            var writer = new StringWriter();
            _serializer.Serialize(writer, document);

            return writer.ToString();
        }

        public T FromJson<T>(string json)
        {
            return _serializer.Deserialize<T>(new JsonTextReader(new StringReader(json)));
        }

        public T FromJson<T>(Stream stream)
        {
            return _serializer.Deserialize<T>(new JsonTextReader(new StreamReader(stream)));
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
            throw new NotImplementedException();
        }

        internal class Route
        {
            public Type HandlerType;
            public MethodInfo Method;
            public string Url;
            public string HttpMethod;
            public Type InputType;
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

        public void Dispose()
        {
            Container?.Dispose();
        }
    }
}