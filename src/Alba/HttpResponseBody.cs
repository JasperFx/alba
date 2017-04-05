using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Baseline;
using Microsoft.AspNetCore.Http;

namespace Alba
{
    public class HttpResponseBody
    {
        private readonly ISystemUnderTest _system;
        private readonly Stream _stream;
        private HttpResponse _response;

        public HttpResponseBody(ISystemUnderTest system, HttpContext context)
        {
            _system = system;
            _stream = context.Response.Body;
            _response = context.Response;
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
            _stream.Position = 0;
            return read(_stream);
        }

        /// <summary>
        /// Read the contents of the HttpResponse.Body into an XmlDocument object
        /// </summary>
        /// <returns></returns>
        public XmlDocument ReadAsXml()
        {
            Func<Stream, XmlDocument> read = s =>
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
        public T ReadAsXml<T>() where T : class
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
            var json = ReadAsText();
            return _system.FromJson<T>(json);
        }
    }
}