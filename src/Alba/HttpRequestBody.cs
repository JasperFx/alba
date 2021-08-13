using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
#nullable enable
namespace Alba
{
    public class HttpRequestBody
    {
        private readonly IAlbaHost _system;
        private readonly Scenario _parent;

        internal HttpRequestBody(IAlbaHost system, Scenario parent)
        {
            _system = system;
            _parent = parent;
        }

        public void XmlInputIs(object target)
        {
            var writer = new StringWriter();

            var serializer = new XmlSerializer(target.GetType());
            serializer.Serialize(writer, target);
            var xml = writer.ToString();
            var bytes = Encoding.UTF8.GetBytes(xml);

            _parent.Configure = context =>
            {
                var stream = context.Request.Body;
                stream.Write(bytes, 0, bytes.Length);
                stream.Position = 0;

                context.Request.ContentType = MimeType.Xml.Value;
                context.Accepts(MimeType.Xml.Value);
                context.Request.ContentLength = xml.Length;
            };


        }

        private void writeTextToBody(string json, HttpContext context)
        {
            var stream = context.Request.Body;

            var writer = new StreamWriter(stream);
            writer.Write(json);
            writer.Flush();

            stream.Position = 0;

            context.Request.ContentLength = stream.Length;
        }

        public void WriteFormData(Dictionary<string, string> input)
        {
            _parent.Configure = context =>
            {
                context.Request.ContentType(MimeType.HttpFormMimetype);
                context.WriteFormData(input);
            };


        }

        public void TextIs(string body)
        {
            _parent.Configure = context =>
            {
                writeTextToBody(body, context);
                context.Request.ContentType = MimeType.Text.Value;
                context.Request.ContentLength = body.Length;
            };


        }
    }
}