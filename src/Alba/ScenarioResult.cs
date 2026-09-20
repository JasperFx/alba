using System.Net.ServerSentEvents;
using System.Text.Json;
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
    public byte[] ReadAsBytes()
    {
        return Read(s =>
        {
            var buffer = new MemoryStream();
            s.CopyTo(buffer);
            return buffer.ToArray();
        });
    }

    /// <inheritdoc />
    public Task<byte[]> ReadAsBytesAsync()
    {
        return ReadAsync(async s =>
        {
            var buffer = new MemoryStream();
            await s.CopyToAsync(buffer);
            return buffer.ToArray();
        });
    }

    /// <inheritdoc />
    public XmlDocument? ReadAsXml()
    {
        return Read(s => tryParseXml(s.ReadAllText()));
    }

    /// <inheritdoc />
    public Task<XmlDocument?> ReadAsXmlAsync()
    {
        return ReadAsync(async s => tryParseXml(await s.ReadAllTextAsync()));
    }

    private static XmlDocument? tryParseXml(string body)
    {
        var document = new XmlDocument();
        try
        {
            document.LoadXml(body);
        }
        catch (XmlException)
        {
            return null;
        }

        return document;
    }

    /// <inheritdoc />
    public T? ReadAsXml<T>() where T : class
    {
        return Read(s => new XmlSerializer(typeof(T)).Deserialize(s) as T);
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

    /// <inheritdoc />
    public IReadOnlyList<SseItem<string>> ReadAsServerSentEvents()
    {
        // The response body is a fully buffered MemoryStream, so the
        // synchronous parse never touches server streams
        return Read(s => SseParser.Create(s).Enumerate().ToList());
    }

    /// <inheritdoc />
    public IReadOnlyList<SseItem<T>> ReadAsServerSentEvents<T>()
    {
        return Read(s => SseParser
            .Create(s, (_, data) => JsonSerializer.Deserialize<T>(data, _system.StjJsonOptions)!)
            .Enumerate().ToList());
    }

    /// <summary>
    /// Read the buffered response body from the start, leaving it rewound for
    /// the next read
    /// </summary>
    public T Read<T>(Func<Stream, T> read)
    {
        var body = Context.Response.Body;
        body.Position = 0;

        try
        {
            return read(body);
        }
        finally
        {
            body.Position = 0;
        }
    }

    /// <summary>
    /// Read the buffered response body from the start, leaving it rewound for
    /// the next read
    /// </summary>
    public async Task<T> ReadAsync<T>(Func<Stream, Task<T>> read)
    {
        var body = Context.Response.Body;
        body.Position = 0;

        try
        {
            return await read(body);
        }
        finally
        {
            body.Position = 0;
        }
    }
}
