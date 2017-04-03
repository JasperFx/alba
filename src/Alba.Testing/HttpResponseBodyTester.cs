using System.Xml.Serialization;
using Alba.Stubs;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class HttpResponseBodyTester
    {
        [Fact]
        public void can_parse_xml()
        {
            var context = StubHttpContext.Empty();

            var serializer = new XmlSerializer(typeof(MyMessage));
            serializer.Serialize(context.Response.Body, new MyMessage {Age = 3, Name = "Declan"});

            context.Response.Body.Position = 0;

            var body = new HttpResponseBody(null, context);

            var message = body.ReadAsXml<MyMessage>();
            message.Name.ShouldBe("Declan");
        }

        [Fact]
        public void can_get_raw_xml_document()
        {
            var context = StubHttpContext.Empty();

            var serializer = new XmlSerializer(typeof(MyMessage));
            serializer.Serialize(context.Response.Body, new MyMessage { Age = 3, Name = "Declan" });

            context.Response.Body.Position = 0;

            var body = new HttpResponseBody(null, context);

            var root = body.ReadAsXml();

            root.DocumentElement["Name"].InnerText.ShouldBe("Declan");
        }

        public class MyMessage
        {
            public string Name;
            public int Age;
        }

        
    }
}