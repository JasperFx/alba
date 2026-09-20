using System.Text;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;

namespace Alba;

public class HttpRequestBody
{
    private readonly Scenario _parent;

    internal HttpRequestBody(Scenario parent)
    {
        _parent = parent;
    }

    public void XmlInputIs(object target)
    {
        using var writer = new StringWriter();

        var serializer = new XmlSerializer(target.GetType());
        serializer.Serialize(writer, target);
        var xml = writer.ToString();

        _parent.ConfigureHttpContext(context =>
        {
            WriteTextToBody(xml, context);
            context.Request.ContentType = MimeType.Xml.Value;
            context.Accepts(MimeType.Xml.Value);
        });
    }

    private static void WriteTextToBody(string text, HttpContext context)
    {
        var stream = context.Request.Body;
        var bytes = Encoding.UTF8.GetBytes(text);

        stream.Write(bytes, 0, bytes.Length);
        stream.Position = 0;

        context.Request.ContentLength = stream.Length;
    }

    public void WriteFormData(Dictionary<string, string> input)
    {
        _parent.ConfigureHttpContext(context =>
        {
            context.WriteFormData(input);
        });
    }

    public void WriteMultipartFormData(MultipartFormDataContent content)
    {
        _parent.ConfigureHttpContext(context =>
        {
            context.WriteMultipartFormData(content);
        });
    }

    public void TextIs(string body)
    {
        _parent.ConfigureHttpContext(context =>
        {
            WriteTextToBody(body, context);
            context.Request.ContentType = MimeType.Text.Value;
        });
    }
}
