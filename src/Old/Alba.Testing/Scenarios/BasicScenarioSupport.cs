using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Alba.Routing;
using Alba.Scenarios;
using Newtonsoft.Json;

namespace Alba.Testing.Scenarios
{
    public class BasicScenarioSupport : IScenarioSupport
    {
        private readonly JsonSerializer _serializer = new JsonSerializer
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };

        public readonly Router Router = new Router();

        public string RootUrl { get; } = string.Empty;
        public T Get<T>()
        {
            throw new NotSupportedException();
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

        public Task Invoke(Dictionary<string, object> env)
        {
            return Router.Invoke(env);
        }

        public IUrlRegistry Urls => Router.Urls;
    }
}