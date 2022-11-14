using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using Baseline;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Alba
{
    public class ScenarioResult : IScenarioResult
    {
        private readonly AlbaHost _system;

        internal ScenarioResult(AlbaHost system, HttpContext context)
        {
            _system = system;
            Context = context;
        }

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
            if (Context.Response.Body.CanSeek || Context.Response.Body is MemoryStream)
            {
                Context.Response.Body.Position = 0;
            }
            else
            {
                var stream = new MemoryStream();
                Context.Response.Body.CopyTo(stream);
                stream.Position = 0;
                Context.Response.Body = stream;
            }
            
            var returnValue = read(Context.Response.Body);
            Context.Response.Body.Position = 0;

            return returnValue;
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
            return _system.DefaultJson.Read<T>(this);
        }

    }
}