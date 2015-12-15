using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Baseline;

namespace Alba.Scenarios
{
    // TODO -- test through the scenario support
    // TODO -- add headers, status code, status description, cookies
    public class HttpResponseBody
    {
        private readonly IScenarioSupport _support;
        private readonly Stream _stream;

        public HttpResponseBody(IScenarioSupport support, IDictionary<string, object> environment)
        {
            _support = support;
            _stream = environment.ResponseStream();
            OwinRequest = environment;
        }

        public IDictionary<string, object> OwinRequest { get; }

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
            return _support.FromJson<T>(json);
        }
    }
}