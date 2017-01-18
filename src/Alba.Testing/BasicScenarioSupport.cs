using System;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Alba.Stubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json;

namespace Alba.Testing
{
    public class BasicScenarioSupport : ISystemUnderTest
    {
        private readonly JsonSerializer _serializer = new JsonSerializer
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };

        public HttpContext CreateContext()
        {
            return new StubHttpContext(Features, Services);
        }

        public IFeatureCollection Features { get; } = null;
        public IServiceProvider Services { get; set; } = null;
        public RequestDelegate Invoker { get; set; }
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

        public string UrlFor<T>(Expression<Action<T>> expression, string method)
        {
            throw new NotImplementedException();
        }

        public string UrlFor<T>(string method)
        {
            throw new NotImplementedException();
        }

        public string UrlFor<T>(T input, string method)
        {
            throw new NotImplementedException();
        }
    }
}