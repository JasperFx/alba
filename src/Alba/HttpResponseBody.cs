using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Baseline;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Alba
{
    public class HttpResponseBody
    {
        private readonly IAlbaHost _system;
        private readonly Stream _stream;
        private HttpResponse _response;
        private readonly HttpContext _context;

        internal HttpResponseBody(IAlbaHost system, HttpContext context)
        {
            _system = system;
            _stream = context.Response.Body;
            _response = context.Response;
            _context = context;
        }

        public T Read<T>(string contentType)
        {
            
            _context.Response.Body.Position = 0;
            
            // TODO -- memoize the formatters
            var options = _system.Services.GetRequiredService<IOptions<MvcOptions>>();
            var formatter = options.Value.InputFormatters.OfType<InputFormatter>()
                .FirstOrDefault(x => x.SupportedMediaTypes.Contains(contentType));

            IModelMetadataProvider provider = _system.Services.GetRequiredService<IModelMetadataProvider>();
            var metadata = provider.GetMetadataForType(typeof(T));
            var context = new InputFormatterContext(_context, typeof(T).Name, new ModelStateDictionary(), metadata, (s, e) => new StreamReader(_context.Response.Body));

            var result = formatter.ReadAsync(context).GetAwaiter().GetResult();

            return (T) result.Model;
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
            if (_stream.CanSeek)
            {
                _stream.Position = 0;
            }
            return read(_stream);
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
            _stream.Position = 0;
            var serializer = new XmlSerializer(typeof (T));
            return serializer.Deserialize(_stream) as T;
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
            // var json = ReadAsText();
            // return _system.FromJson<T>(json);
        }
    }
}