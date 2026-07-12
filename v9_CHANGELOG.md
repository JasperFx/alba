# Alba v9 Changelog

## Breaking changes

### .NET 10 is required

Alba now targets `net10.0` only; support for .NET 8 and .NET 9 is dropped. Applications under
test must run on .NET 10.

### `BeforeEachAsync` now receives the `Scenario` and runs before the request

`IAlbaHost.BeforeEachAsync` changed from `Func<HttpContext, Task>` to `Func<Scenario, Task>`. In
Alba 8 the hook received the `HttpContext` and was executed by blocking a thread inside the test
server; it is now properly awaited *before* the HTTP request executes.

If your hook touched the `HttpContext`, do the asynchronous work first, then apply the result to
the outgoing request:

```cs
// Alba 8
host.BeforeEachAsync(async context =>
{
    var token = await FetchTokenAsync();
    context.SetBearerToken(token);
});

// Alba 9
host.BeforeEachAsync(async scenario =>
{
    var token = await FetchTokenAsync();
    scenario.WithBearerToken(token);
});
```

For arbitrary context mutations, register a callback with `scenario.ConfigureHttpContext(...)`.

Ordering: all asynchronous `BeforeEachAsync` actions run first (in registration order), then all
synchronous `BeforeEach` actions (in registration order). In Alba 8 the two kinds interleaved in
registration order.

### Synchronous bootstrapping removed

The `new AlbaHost(IHostBuilder)` constructor, the `StartAlba()` extension method, and the
synchronous `AlbaHost.For(Action<IWebHostBuilder>)` overload are removed. Bootstrapping is
asynchronous only:

```cs
await using var host = await AlbaHost.For(hostBuilder);
// or
await using var host = await hostBuilder.StartAlbaAsync();
```

Use your test framework's asynchronous initialization support (e.g. xUnit's `IAsyncLifetime`) to
start the host.

### `AllowSynchronousIO` is no longer forced on

Alba no longer performs any synchronous I/O against server streams and no longer sets
`AllowSynchronousIO = true` on the `TestServer`. If the application under test performs
synchronous reads or writes against the request/response streams in its own pipeline, opt back
in explicitly after starting the host:

```cs
host.Server.AllowSynchronousIO = true;
```

### Extension model redesigned around `IAlbaHostBuilder`

`IAlbaExtension.Configure` changed from `IHostBuilder Configure(IHostBuilder builder)` to
`void Configure(IAlbaHostBuilder builder)`. The new `IAlbaHostBuilder` surface is
hosting-model-agnostic and behaves identically for every bootstrapping style (`IHostBuilder`,
`WebApplicationBuilder`, and `WebApplicationFactory`); in Alba 8 the `WebApplicationBuilder` path
went through a partial `IHostBuilder` facade and the `WebApplicationFactory` path silently ignored
the returned builder.

```cs
// Alba 8
public IHostBuilder Configure(IHostBuilder builder)
{
    return builder.ConfigureServices(services => services.AddSingleton<MyStub>());
}

// Alba 9
public void Configure(IAlbaHostBuilder builder)
{
    builder.ConfigureServices(services => services.AddSingleton<MyStub>());
}
```

Configuration overrides use `builder.ConfigureConfiguration(c => c.AddInMemoryCollection(...))`,
which takes precedence over the application's own configuration on every path. `Configure` now has
a default no-op implementation, so extensions that only need post-start logic can implement
`Start` alone.

### `IJsonStrategy.Write` is now asynchronous

`IJsonStrategy.Write<T>(T body)` changed to `Task<Stream> WriteAsync<T>(T body)`. This only
affects custom `IJsonStrategy` implementations; request JSON serialization now happens in an
awaited preparation phase before the request executes instead of blocking inside the test
server's setup callback.

### `IdentityModel` replaced by `Duende.IdentityModel`

Alba's OpenID Connect extensions now use the `Duende.IdentityModel` package (the renamed successor
of `IdentityModel`). This is visible in the public API of `OpenConnectExtension` and its
subclasses: types like `TokenResponse` and `DiscoveryDocumentResponse` now come from the
`Duende.IdentityModel.Client` namespace. If you override `FetchToken` or consume the token types,
update your `using IdentityModel.Client;` directives to `using Duende.IdentityModel.Client;`.

### `HttpResponse.Write` extension removed

The `Alba.HttpContextExtensions.Write(this HttpResponse, string)` helper performed synchronous
stream writes. Use ASP.NET Core's built-in `HttpResponse.WriteAsync(...)` instead.

### `Dispose()` performs a full, awaited shutdown

`IAlbaHost.Dispose()` previously fired the host stop without awaiting it. It now performs the
complete teardown (equivalent to awaiting `DisposeAsync()`), including disposing extensions via
`DisposeAsync`. Prefer `await using` / `DisposeAsync()` in new code.

### `ReadAsXml` returns null on unparseable bodies instead of matching on "Error"

`IScenarioResult.ReadAsXml()` / `ReadAsXmlAsync()` previously returned `null` whenever the response
body merely *contained* the substring "Error", and threw on any other unparseable body. They now
attempt to parse and return `null` only when the body is not valid XML. Valid XML documents that
happen to contain the text "Error" now parse successfully.

## Improvements

- The response body is buffered once, asynchronously, immediately after each request completes.
  All response reads — including the synchronous `ReadAsText()`, `ReadAsJson<T>()`, and body
  assertions — are seekable, repeatable, memory-only operations.
- Scenario setup exceptions from asynchronous before-each actions surface directly with their
  original stack traces instead of being marshalled out of the test server callback.
- **Server-sent events support.** Finite streams: `IScenarioResult.ReadAsServerSentEvents()` and
  `ReadAsServerSentEvents<T>()` parse a buffered response body as `SseItem<T>` values, with the
  typed overload deserializing each data payload through the application's JSON options. Live
  streams: `IAlbaHost.StreamServerSentEvents(Action<Scenario>, CancellationToken)` opens an
  unbuffered event stream that yields events as the application writes them; all
  `BeforeEach`/`BeforeEachAsync` actions and the security extensions apply exactly as they do for
  scenarios. Disposing the returned `SseStreamResult` aborts the request on the server (cancelling
  `HttpContext.RequestAborted`) and then runs `AfterEach`/`AfterEachAsync` actions with a `null`
  `HttpContext`. Response assertions other than the expected status code are not supported on the
  streaming path and are rejected up front.
