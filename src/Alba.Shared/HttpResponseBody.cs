using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Baseline.Testing;

namespace Alba
{
    // TODO -- test through the scenario support
    public class HttpResponseBody
    {
        private readonly Stream _stream;
        private readonly IDictionary<string, object> env;

        public HttpResponseBody(Stream stream, IDictionary<string, object> environment)
        {
            _stream = stream;
            env = environment;
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
            Func<Stream, XmlDocument> read = s => {
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
            var serializer = new XmlSerializer(typeof(T));
            return serializer.Deserialize(_stream) as T;
        }

        public T ReadAsJson<T>()
        {
            throw new NotImplementedException("Redo");
            /*
            var json = ReadAsText();

            if (env.ContainsKey("scenario-support"))
            {
                return
                    env.Get<IScenarioSupport>("scenario-support")
                        .Get<IJsonSerializer>()
                        .Deserialize<T>(json);
            }


            return JsonUtil.Get<T>(json);
            */
        }
    }
}