using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Baseline;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
#nullable enable
namespace Alba
{
    public class HttpResponseBody : IScenarioResult
    {
        private readonly AlbaHost _system;

        internal HttpResponseBody(AlbaHost system, HttpContext context)
        {
            _system = system;
            Context = context;
        }

        [Obsolete("Use the methods directly on IScenarioResult instead")]
        public HttpResponseBody ResponseBody => this;
        
        public HttpContext Context { get; }

        /// <summary>
        /// Read the contents of the HttpResponse.Body as text
        /// </summary>
        /// <returns></returns>
        public string ReadAsText()
        {
            return Read(s => s.ReadAllText());
        }

        public T Read<T>(Func<Stream, T> read)
        {
            if (Context.Response.Body.CanSeek)
            {
                Context.Response.Body.Position = 0;
            }
            return read(Context.Response.Body);
        }

        /// <summary>
        /// Read the contents of the HttpResponse.Body into an XmlDocument object
        /// </summary>
        /// <returns></returns>
        public XmlDocument? ReadAsXml()
        {
            Func<Stream, XmlDocument?> read = s =>
            {
                var body = s.ReadAllText();

                if (body.Contains("Error")) return null;

                var document = new XmlDocument();
                document.LoadXml(body);

                return document;
            };

            return Read(read);
        }

        /// <summary>
        /// Deserialize the contents of the HttpResponse.Body into an object
        /// of type T using the built in XmlSerializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T? ReadAsXml<T>() where T : class
        {
            Context.Response.Body.Position = 0;
            var serializer = new XmlSerializer(typeof (T));
            return serializer.Deserialize(Context.Response.Body) as T;
        }

        /// <summary>
        /// Deserialize the contents of the HttpResponse.Body into an object
        /// of type T using the configured Json serializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T? ReadAsJson<T>()
        {
            return Read<T>(MimeType.Json.Value);
        }

        public T? Read<T>(string contentType)
        {
            var formatter = _system.Inputs[contentType];
            if (formatter == null)
            {
                throw new InvalidOperationException(
                    $"Alba was not able to find a registered formatter for content type '{contentType}'. Either specify the body contents explicitly, or try registering 'services.AddMvcCore()'");
            }

            var provider = _system.Services.GetRequiredService<IModelMetadataProvider>();
            var metadata = provider.GetMetadataForType(typeof(T));

            var standinContext = new DefaultHttpContext();
            standinContext.Request.Body = Context.Response.Body; // Need to trick the MVC conneg services
            var inputContext = new InputFormatterContext(standinContext, typeof(T).Name, new ModelStateDictionary(), metadata, (s, e) => new StreamReader(s));
            var result = formatter.ReadAsync(inputContext).GetAwaiter().GetResult();

            if (result.Model is T returnValue) return returnValue;

            return default(T);
        }
    }
}