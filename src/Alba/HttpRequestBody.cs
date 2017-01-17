using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Baseline;
using Microsoft.AspNetCore.Http;

namespace Alba
{
    public class HttpRequestBody
    {
        private readonly ISystemUnderTest _system;
        private readonly HttpContext _parent;

        public HttpRequestBody(ISystemUnderTest system, HttpContext parent)
        {
            _system = system;
            _parent = parent;
        }

        public void XmlInputIs(object target)
        {
            var serializer = new XmlSerializer(target.GetType());
            var stream = _parent.Response.Body;
            serializer.Serialize(stream, target);
            stream.Position = 0;
        }

        public void JsonInputIs(object target)
        {
            string json = _system.ToJson(target);

            JsonInputIs(json);
        }

        public void JsonInputIs(string json)
        {
            var stream = _parent.Response.Body;

            var writer = new StreamWriter(stream);
            writer.Write(json);
            writer.Flush();

            stream.Position = 0;
        }

        public void WriteFormData<T>(T target) where T : class
        {
            _parent.Request.ContentType(MimeType.HttpFormMimetype);


            var values = new Dictionary<string, string>();

            typeof (T).GetProperties().Where(x => x.CanWrite && x.CanRead).Each(prop =>
            {
                var rawValue = prop.GetValue(target, null);

                values.Add(prop.Name, rawValue?.ToString() ?? string.Empty);
            });

            typeof (T).GetFields().Each(field =>
            {
                var rawValue = field.GetValue(target);

                values.Add(field.Name, rawValue?.ToString() ?? string.Empty);
            });

            _parent.WriteFormData(values);
        }

        public void ReplaceBody(Stream stream)
        {
            stream.Position = 0;
            _parent.Request.Body = stream;
        }

    }
}