using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Alba.Pooling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace Alba.Extensions;

/// <summary>
/// Alba extension that answers the application's outbound HTTP calls made through
/// IHttpClientFactory (named, typed and default clients) with stubbed responses, and records
/// every request. Requests that match no stubbed response throw inside the application.
/// In an AlbaHostPool, returning the host restores the stubbed responses it joined the pool with.
/// </summary>
public sealed class HttpClientStub : IResettableAlbaExtension
{
    private readonly object _lock = new();
    private readonly List<RecordedHttpRequest> _requests = new();

    // Replaced rather than mutated, so requests match against a snapshot without locking
    private HttpClientStubRule[] _rules = [];
    private HttpClientStubRule[] _checkpointRules = [];

    /// <summary>
    /// Stub the client with this name, or every client from IHttpClientFactory when no name
    /// is given. A typed client's name is its class name.
    /// </summary>
    /// <param name="clientName"></param>
    public HttpClientStub(string? clientName = null)
    {
        ClientName = clientName;
    }

    /// <summary>
    /// The stubbed client, or null for every client
    /// </summary>
    public string? ClientName { get; }

    /// <summary>
    /// Serializer settings for JSON responses and for reading recorded request bodies.
    /// The web defaults (camel case) by default.
    /// </summary>
    public JsonSerializerOptions JsonOptions { get; set; } = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Every request the application has sent through the stubbed clients, including
    /// requests that matched no stubbed response
    /// </summary>
    public IReadOnlyList<RecordedHttpRequest> Requests
    {
        get
        {
            lock (_lock) return _requests.ToArray();
        }
    }

    /// <summary>
    /// Stub the response to requests with this method and path. The path is matched without
    /// regard to case, and the query string is only compared when the path includes one.
    /// An absolute url also matches the scheme and host. When several stubbed responses match
    /// a request, the one added last wins.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="path">A path such as "/charges", or an absolute url</param>
    /// <returns></returns>
    public HttpClientStubRule When(HttpMethod method, string path)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var absolute = Uri.TryCreate(path, UriKind.Absolute, out var uri) && uri.Scheme.StartsWith("http");
        var compareQuery = path.Contains('?');

        Func<Uri, string> comparedPart;
        if (absolute) comparedPart = u => u.GetLeftPart(compareQuery ? UriPartial.Query : UriPartial.Path);
        else if (compareQuery) comparedPart = u => u.PathAndQuery;
        else comparedPart = u => u.AbsolutePath;

        return addRule($"{method} {path}", request =>
            request.Method == method
            && string.Equals(comparedPart(request.Uri), path, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Stub the response to requests that match the predicate. When several stubbed responses
    /// match a request, the one added last wins.
    /// </summary>
    /// <param name="match"></param>
    /// <param name="description">Describes the match in error messages</param>
    /// <returns></returns>
    public HttpClientStubRule When(Func<RecordedHttpRequest, bool> match, string description = "a custom match")
    {
        ArgumentNullException.ThrowIfNull(match);
        return addRule(description, match);
    }

    /// <summary>
    /// Remove the recorded requests and keep the stubbed responses
    /// </summary>
    public void ClearRequests()
    {
        lock (_lock) _requests.Clear();
    }

    /// <summary>
    /// Remove the recorded requests and every stubbed response
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            _requests.Clear();
            _rules = [];
        }
    }

    private HttpClientStubRule addRule(string description, Func<RecordedHttpRequest, bool> match)
    {
        var rule = new HttpClientStubRule(this, description, match);
        lock (_lock) _rules = [.. _rules, rule];
        return rule;
    }

    private async Task<HttpResponseMessage> sendAsync(string clientName, HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var body = request.Content == null ? [] : await request.Content.ReadAsByteArrayAsync(cancellationToken);
        var recorded = new RecordedHttpRequest(clientName, request, body, JsonOptions);

        lock (_lock) _requests.Add(recorded);

        var rules = _rules;
        var rule = rules.LastOrDefault(x => x.Matches(recorded));
        if (rule == null)
        {
            var known = rules.Length == 0
                ? "No responses are stubbed."
                : "Stubbed responses: " + string.Join(", ", rules.Select(x => x.Description));
            throw new InvalidOperationException(
                $"No stubbed response for {request.Method} {request.RequestUri} sent through the HttpClient '{clientName}'. {known}");
        }

        var response = rule.CreateResponse(recorded);
        response.RequestMessage ??= request;
        return response;
    }

    void IAlbaExtension.Configure(IAlbaHostBuilder builder)
    {
        // Post configuration runs after the application's own handler configuration,
        // so the stub replaces any primary handler the application sets
        builder.ConfigureServices(services =>
        {
            if (ClientName == null)
            {
                services.PostConfigureAll<HttpClientFactoryOptions>(useStub);
            }
            else
            {
                services.PostConfigure<HttpClientFactoryOptions>(ClientName, useStub);
            }
        });
    }

    private void useStub(HttpClientFactoryOptions options)
    {
        // A handler per pipeline, because the factory disposes expired pipelines
        options.HttpMessageHandlerBuilderActions.Add(b => b.PrimaryHandler = new StubHandler(this, b.Name ?? ""));
    }

    Task IAlbaExtension.Start(IAlbaHost host) => Task.CompletedTask;

    void IResettableAlbaExtension.Checkpoint()
    {
        lock (_lock) _checkpointRules = _rules;
    }

    Task IResettableAlbaExtension.ResetAsync()
    {
        lock (_lock)
        {
            _requests.Clear();
            _rules = _checkpointRules;
        }

        return Task.CompletedTask;
    }

    void IDisposable.Dispose()
    {
    }

    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;

    private sealed class StubHandler(HttpClientStub stub, string clientName) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return stub.sendAsync(clientName, request, cancellationToken);
        }
    }
}

/// <summary>
/// A stubbed response for the outbound requests that match it. Responds with an empty
/// 200 until configured otherwise.
/// </summary>
public sealed class HttpClientStubRule
{
    private readonly HttpClientStub _parent;
    private readonly Func<RecordedHttpRequest, bool> _match;
    private Func<RecordedHttpRequest, HttpResponseMessage> _respond = _ => new HttpResponseMessage(HttpStatusCode.OK);

    internal HttpClientStubRule(HttpClientStub parent, string description, Func<RecordedHttpRequest, bool> match)
    {
        _parent = parent;
        Description = description;
        _match = match;
    }

    internal string Description { get; }

    internal bool Matches(RecordedHttpRequest request) => _match(request);

    internal HttpResponseMessage CreateResponse(RecordedHttpRequest request) => _respond(request);

    /// <summary>
    /// Respond with an empty body and this status code
    /// </summary>
    public HttpClientStub RespondWith(HttpStatusCode statusCode)
    {
        return RespondWith(_ => new HttpResponseMessage(statusCode));
    }

    /// <summary>
    /// Respond with the value serialized as JSON
    /// </summary>
    public HttpClientStub RespondWithJson<T>(T body, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var json = JsonSerializer.Serialize(body, _parent.JsonOptions);
        return RespondWithText(json, statusCode, MimeType.Json.Value);
    }

    /// <summary>
    /// Respond with this text
    /// </summary>
    public HttpClientStub RespondWithText(string text, HttpStatusCode statusCode = HttpStatusCode.OK,
        string contentType = "text/plain")
    {
        return RespondWith(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(text, Encoding.UTF8, contentType)
        });
    }

    /// <summary>
    /// Build each response from the request
    /// </summary>
    public HttpClientStub RespondWith(Func<RecordedHttpRequest, HttpResponseMessage> respond)
    {
        _respond = respond ?? throw new ArgumentNullException(nameof(respond));
        return _parent;
    }

    /// <summary>
    /// Throw the exception from the HttpClient call, for example an HttpRequestException
    /// to simulate a network failure
    /// </summary>
    public HttpClientStub Throws(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return RespondWith(_ => throw exception);
    }
}

/// <summary>
/// An outbound request the application sent through a stubbed HttpClient
/// </summary>
public sealed class RecordedHttpRequest
{
    private readonly JsonSerializerOptions _jsonOptions;

    internal RecordedHttpRequest(string clientName, HttpRequestMessage request, byte[] body,
        JsonSerializerOptions jsonOptions)
    {
        ClientName = clientName;
        Method = request.Method;
        Uri = request.RequestUri ?? new Uri("/", UriKind.Relative);
        Headers = request.Headers;
        ContentType = request.Content?.Headers.ContentType;
        Body = body;
        _jsonOptions = jsonOptions;
    }

    /// <summary>
    /// The name of the HttpClient that sent the request
    /// </summary>
    public string ClientName { get; }

    public HttpMethod Method { get; }

    public Uri Uri { get; }

    public HttpRequestHeaders Headers { get; }

    public MediaTypeHeaderValue? ContentType { get; }

    /// <summary>
    /// The raw request body, empty when the request had none
    /// </summary>
    public byte[] Body { get; }

    /// <summary>
    /// Read the request body as UTF-8 text
    /// </summary>
    public string ReadAsText() => Encoding.UTF8.GetString(Body);

    /// <summary>
    /// Deserialize the request body as JSON with the stub's JsonOptions
    /// </summary>
    public T? ReadAsJson<T>() => JsonSerializer.Deserialize<T>(Body, _jsonOptions);

    public override string ToString() => $"{Method} {Uri}";
}
