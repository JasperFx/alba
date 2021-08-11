using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;

namespace Alba
{
    [Obsolete("Tighten up the usage")]
    public class HttpRequestBody
    {
        private readonly IAlbaHost _host;
        private readonly Scenario _parent;

        internal HttpRequestBody(IAlbaHost host, Scenario parent)
        {
            _host = host;
            _parent = parent;
        }

        public void XmlInputIs(object target)
        {
            var writer = new StringWriter();

            var serializer = new XmlSerializer(target.GetType());
            serializer.Serialize(writer, target);
            var xml = writer.ToString();
            var bytes = Encoding.UTF8.GetBytes(xml);

            _parent.ConfigureHttpContext(context =>
            {
                var stream = context.Request.Body;
                stream.Write(bytes, 0, bytes.Length);
                stream.Position = 0;

                context.Request.ContentType = MimeType.Xml.Value;
                context.Accepts(MimeType.Xml.Value);
                context.Request.ContentLength = xml.Length;
            });


        }

        public void JsonInputIs<T>(T target)
        {
            _parent.WithInputBody(target, "application/json");
        }

        public void JsonInputIs(string json)
        {
            

            _parent.ConfigureHttpContext(context =>
            {
                writeTextToBody(json, context);
                
                context.Request.ContentType = MimeType.Json.Value;
                context.Accepts(MimeType.Json.Value);
                context.Request.ContentLength = json.Length;
            });


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
            _parent.ConfigureHttpContext(context =>
            {
                context.Request.ContentType(MimeType.HttpFormMimetype);
                context.WriteFormData(input);
            });


        }

        public void ReplaceBody(Stream stream)
        {
            _parent.ConfigureHttpContext(context =>
            {
                stream.Position = 0;
                context.Request.Body = stream;
            });


        }

        public void TextIs(string body)
        {
            _parent.ConfigureHttpContext(context =>
            {
                writeTextToBody(body, context);
                context.Request.ContentType = MimeType.Text.Value;
                context.Request.ContentLength = body.Length;
            });


        }
    }
}