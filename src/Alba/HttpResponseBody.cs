using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Alba.Stubs;
using Baseline;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Alba
{
    public class HttpResponseBody
    {
        private readonly IAlbaHost _system;
        private readonly HttpContext _context;

        internal HttpResponseBody(IAlbaHost system, HttpContext context)
        {
            _system = system;
            _context = context;
        }

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
            if (_context.Response.Body.CanSeek)
            {
                _context.Response.Body.Position = 0;
            }
            return read(_context.Response.Body);
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
            _context.Response.Body.Position = 0;
            var serializer = new XmlSerializer(typeof (T));
            return serializer.Deserialize(_context.Response.Body) as T;
        }

        /// <summary>
        /// Deserialize the contents of the HttpResponse.Body into an object
        /// of type T using the configured Json serializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ReadAsJson<T>()
        {
            return Read<T>(MimeType.Json.Value);
        }

        public T Read<T>(string contentType)
        {
            // TODO -- memoize things
            var options = _system.Services.GetRequiredService<IOptionsMonitor<MvcOptions>>();
            var formatter = options.Get("").InputFormatters.OfType<InputFormatter>()
                .FirstOrDefault(x => x.SupportedMediaTypes.Contains(contentType));

            if (formatter == null)
            {
                throw new InvalidOperationException(
                    $"Alba was not able to find a registered formatter for content type '{contentType}'. Either specify the body contents explicitly, or try registering 'services.AddMvcCore()'");
            }

            var provider = _system.Services.GetRequiredService<IModelMetadataProvider>();
            var metadata = provider.GetMetadataForType(typeof(T));

            var standinContext = new DefaultHttpContext();
            standinContext.Request.Body = _context.Response.Body; // Need to trick the MVC conneg services
            var inputContext = new InputFormatterContext(standinContext, typeof(T).Name, new ModelStateDictionary(), metadata, (s, e) => new StreamReader(s));
            var result = formatter.ReadAsync(inputContext).GetAwaiter().GetResult();

            
            
            return (T)result.Model;
        }
    }
}