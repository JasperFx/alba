using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Baseline;
using Microsoft.AspNetCore.Http;

namespace Alba
{
    // TODO -- test through the scenario support
    // TODO -- add headers, status code, status description, cookies
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

        public string ReadAsText()
        {
            return Read(s => s.ReadAllText());
        }

        public T Read<T>(Func<Stream, T> read)
        {
            _stream.Position = 0;
            return read(_stream);
        }

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

        public T ReadAsXml<T>() where T : class
        {
            _stream.Position = 0;
            var serializer = new XmlSerializer(typeof (T));
            return serializer.Deserialize(_stream) as T;
        }

        public T ReadAsJson<T>()
        {
            var json = ReadAsText();
            return _system.FromJson<T>(json);
        }
    }
}