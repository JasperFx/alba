using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Baseline;

namespace Alba.Scenarios
{
    public class HttpRequestBody
    {
        private readonly IScenarioSupport _support;
        private readonly Dictionary<string, object> _parent;

        public HttpRequestBody(IScenarioSupport support, Dictionary<string, object> parent)
        {
            _support = support;
            _parent = parent;
        }

        public void XmlInputIs(object target)
        {
            var serializer = new XmlSerializer(target.GetType());
            var stream = _parent.RequestBody();
            serializer.Serialize(stream, target);
            stream.Position = 0;
        }

        public void JsonInputIs(object target)
        {
            string json = _support.ToJson(target);

            JsonInputIs(json);
        }

        public void JsonInputIs(string json)
        {
            var stream = _parent.RequestBody();

            var writer = new StreamWriter(stream);
            writer.Write(json);
            writer.Flush();

            stream.Position = 0;
        }

        public void WriteFormData<T>(T target) where T : class
        {
            _parent.RequestHeaders().ContentType(MimeType.HttpFormMimetype);


            var values = new NameValueCollection();

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

            _parent.RequestHeaders().ContentType(MimeType.HttpFormMimetype);
            _parent.WriteFormData(values);
        }

        public void ReplaceBody(Stream stream)
        {
            stream.Position = 0;
            _parent.Append(OwinConstants.RequestBodyKey, stream);
        }

    }
}