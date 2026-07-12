using System.Diagnostics;
using System.Net;
using System.Net.ServerSentEvents;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Alba;

/// <summary>
/// A live server-sent event stream opened through an AlbaHost. Disposing the
/// result aborts the in-flight request on the server.
/// </summary>
public sealed class SseStreamResult : IAsyncDisposable
{
    private readonly AlbaHost _system;
    private readonly Stream _stream;
    private readonly HttpMessageInvoker _invoker;
    private readonly Activity? _activity;
    private readonly IReadOnlyList<Func<HttpContext?, Task>> _afterEach;
    private bool _consumed;
    private bool _disposed;

    internal SseStreamResult(AlbaHost system, HttpResponseMessage response, Stream stream,
        HttpMessageInvoker invoker, Activity? activity, IReadOnlyList<Func<HttpContext?, Task>> afterEach)
    {
        _system = system;
        Response = response;
        _stream = stream;
        _invoker = invoker;
        _activity = activity;
        _afterEach = afterEach;
    }

    /// <summary>
    /// The raw response message for the event stream
    /// </summary>
    public HttpResponseMessage Response { get; }

    public HttpStatusCode StatusCode => Response.StatusCode;

    /// <summary>
    /// Enumerate the server-sent events as they arrive. The stream can only be
    /// consumed once.
    /// </summary>
    public IAsyncEnumerable<SseItem<string>> ReadEvents(CancellationToken cancellation = default)
    {
        return SseParser.Create(claimStream()).EnumerateAsync(cancellation);
    }

    /// <summary>
    /// Enumerate the server-sent events as they arrive, deserializing each data
    /// payload as JSON with the application's serializer options. The stream can
    /// only be consumed once.
    /// </summary>
    public IAsyncEnumerable<SseItem<T>> ReadEvents<T>(CancellationToken cancellation = default)
    {
        var options = _system.StjJsonOptions;
        return SseParser
            .Create(claimStream(), (_, data) => JsonSerializer.Deserialize<T>(data, options)!)
            .EnumerateAsync(cancellation);
    }

    private Stream claimStream()
    {
        if (_consumed)
        {
            throw new InvalidOperationException(
                "The event stream has already been consumed and can only be read once");
        }

        _consumed = true;
        return _stream;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        // Disposing the response aborts the in-flight request, cancelling the
        // server's HttpContext.RequestAborted token
        Response.Dispose();
        _invoker.Dispose();
        _activity?.Dispose();

        foreach (var afterEach in _afterEach) await afterEach(null);
    }
}
