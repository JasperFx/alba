using System.Xml;
using System.Xml.Serialization;
using Alba.Internal;
using Microsoft.AspNetCore.Http;

namespace Alba;

public class ScenarioResult : IScenarioResult
{
    private readonly AlbaHost _system;

    internal ScenarioResult(AlbaHost system, HttpContext context)
    {
        _system = system;
        Context = context;
    }

    public HttpContext Context { get; }

    /// <inheritdoc />
    public string ReadAsText()
    {
        return Read(s => s.ReadAllText());
    }

    /// <inheritdoc />
    public Task<string> ReadAsTextAsync()
    {
        return ReadAsync(s => s.ReadAllTextAsync());
    }

    /// <inheritdoc />
    public XmlDocument? ReadAsXml()
    {
        Func<Stream, XmlDocument?> read = s =>
        {
            var body = s.ReadAllText();

            if (body.Contains("Error"))
            {
                return null;
            }

            var document = new XmlDocument();
            document.LoadXml(body);

            return document;
        };

        return Read(read);
    }

    /// <inheritdoc />
    public Task<XmlDocument?> ReadAsXmlAsync()
    {
        Func<Stream, Task<XmlDocument?>> read = async s =>
        {
            var body = await s.ReadAllTextAsync();

            if (body.Contains("Error"))
            {
                return null;
            }

            var document = new XmlDocument();
            document.LoadXml(body);

            return document;
        };

        return Read(read);
    }

    /// <inheritdoc />
    public T? ReadAsXml<T>() where T : class
    {
        Context.Response.Body.Position = 0;
        var serializer = new XmlSerializer(typeof(T));
        return serializer.Deserialize(Context.Response.Body) as T;
    }

    /// <inheritdoc />
    public T ReadAsJson<T>()
    {
        return _system.DefaultJson.Read<T>(this);
    }

    /// <inheritdoc />
    public Task<T> ReadAsJsonAsync<T>()
    {
        return _system.DefaultJson.ReadAsync<T>(this);
    }

    public T Read<T>(Func<Stream, T> read)
    {
        if (Context.Response.Body.CanSeek || Context.Response.Body is MemoryStream)
        {
            Context.Response.Body.Position = 0;
        }
        else
        {
            var stream = new MemoryStream();
            Context.Response.Body.CopyTo(stream);
            stream.Position = 0;
            Context.Response.Body = stream;
        }

        var returnValue = read(Context.Response.Body);
        Context.Response.Body.Position = 0;

        return returnValue;
    }

    public async Task<T> ReadAsync<T>(Func<Stream, Task<T>> read)
    {
        if (Context.Response.Body.CanSeek || Context.Response.Body is MemoryStream)
        {
            Context.Response.Body.Position = 0;
        }
        else
        {
            var stream = new MemoryStream();
            await Context.Response.Body.CopyToAsync(stream);
            stream.Position = 0;
            Context.Response.Body = stream;
        }

        try
        {
            return await read(Context.Response.Body);
        }
        finally
        {
            Context.Response.Body.Position = 0;
        }
    }
}