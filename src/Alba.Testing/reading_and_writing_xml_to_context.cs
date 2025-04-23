using System.Xml;
using System.Xml.Serialization;
using JasperFx.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Shouldly;

namespace Alba.Testing
{
    public class reading_and_writing_xml_to_context
    {
        [Fact]
        public void can_parse_xml()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var serializer = new XmlSerializer(typeof(MyMessage));
            serializer.Serialize(context.Response.Body, new MyMessage {Age = 3, Name = "Declan"});

            context.Response.Body.Position = 0;

            var body = new ScenarioResult(null, context);

            var message = body.ReadAsXml<MyMessage>();
            message.Name.ShouldBe("Declan");
        }

        [Fact]
        public void can_get_raw_xml_document()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var serializer = new XmlSerializer(typeof(MyMessage));
            serializer.Serialize(context.Response.Body, new MyMessage { Age = 3, Name = "Declan" });

            context.Response.Body.Position = 0;

            var body = new ScenarioResult(null, context);

            var root = body.ReadAsXml();

            root.DocumentElement["Name"].InnerText.ShouldBe("Declan");
        }

        [Fact]
        public void can_write_xml_to_request()
        {
            var context = new DefaultHttpContext();
            using var system = AlbaHost.For(b => 
                b.Configure(app => app.Run(c => c.Response.WriteAsync("Hello"))));
            
            var scenario = new Scenario(system);
            new HttpRequestBody(null, scenario).XmlInputIs(new MyMessage { Age = 3, Name = "Declan" });

            scenario.SetupHttpContext(context);

            context.Request.Body.Position = 0;

            var xml = context.Request.Body.ReadAllText();
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            doc.DocumentElement["Name"].InnerText.ShouldBe("Declan");
        }

        public class MyMessage
        {
            public string Name;
            public int Age;
        }

        
    }
}