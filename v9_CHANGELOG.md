# Alba v9 Changelog

## Breaking changes

### .NET 10 is required

Alba now targets `net10.0` and `net11.0`; support for .NET 8 and .NET 9 is dropped. Applications
under test must run on .NET 10 or later. 

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

### `AlbaHost.For(Action<IWebHostBuilder>)` removed

This overload implicitly created a `Host.CreateDefaultBuilder()` host and could not accept Alba
extensions. Build the host builder explicitly instead:

```cs
// Alba 8
var host = await AlbaHost.For(w => w.UseStartup<Startup>());

// Alba 9
var host = await AlbaHost.For(Host.CreateDefaultBuilder()
    .ConfigureWebHostDefaults(w => w.UseStartup<Startup>()));
```

### Bootstrapping returns an awaitable `AlbaHostBuilder`

`AlbaHost.For(...)`, `AlbaHost.For<T>(...)`, and the `StartAlbaAsync()` extension methods return an
`AlbaHostBuilder` instead of `Task<IAlbaHost>`. The builder is awaitable and the application starts
when it is awaited, so every `await AlbaHost.For<Program>(...)` keeps working. Configuration value
overrides and extensions can be chained onto it first; service registrations and other host
customization stay in the configuration action:

```cs
await using var host = await AlbaHost.For<Program>(x =>
    {
        x.ConfigureServices(s => s.AddSingleton<IExternalWebService>(stub));
    })
    .WithConfiguration("ConnectionStrings:Postgres", "MyOverriddenValue")
    .WithExtension(new AuthenticationStub().WithName("jeremy"));
```

Code that stores the result as a `Task<IAlbaHost>` or passes it where a `Func<Task>` is expected
needs to start the builder explicitly:

```cs
// Alba 8
Task<IAlbaHost> starting = AlbaHost.For<Program>();
await Should.ThrowAsync<Exception>(() => AlbaHost.For<Program>(badExtension));

// Alba 9
Task<IAlbaHost> starting = AlbaHost.For<Program>().StartAsync();
await Should.ThrowAsync<Exception>(async () => await AlbaHost.For<Program>(badExtension));
```

### `ConfigurationOverride` removed

Configuration values are overridden on the builder instead:

```cs
// Alba 8
var host = await AlbaHost.For<Program>(ConfigurationOverride.Create(values));

// Alba 9
var host = await AlbaHost.For<Program>().WithConfiguration(values);
// or one value at a time
var host = await AlbaHost.For<Program>().WithConfiguration("ConnectionStrings:Postgres", "value");
```

### On .NET 11, `AlbaHost.For<T>` requires a `WebApplicationBuilder` application

The `net11.0` build of Alba uses `WebApplicationFactory`'s new `ConfigureWebApplicationBuilder` hook
to apply configuration before the application's own startup code runs. `WebApplicationFactory` only
supports that hook for applications built with `WebApplicationBuilder`, so on .NET 11 bootstrapping
a `Startup.cs`-style application through `AlbaHost.For<T>` fails with an `InvalidOperationException`
from `WebApplicationFactory`. Bootstrap those applications through their `IHostBuilder`, which is
fully supported on every target:

```cs
// .NET 11, Startup.cs-style application
await using var host = await Program.CreateHostBuilder(args).StartAlbaAsync();
```

The `net10.0` build supports both hosting models through `AlbaHost.For<T>` as before.

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

### The default status code expectation accepts any 2xx

Scenarios that do not configure a status code expectation now pass for any status code between
200 and 299 instead of requiring exactly 200, and the default failure message changed to
"Expected a status code between 200 and 299, but was ...". Require one exact code with
`StatusCodeShouldBe(...)` or `StatusCodeShouldBeOk()`:

```cs
await host.Scenario(x =>
{
    x.Get.Url("/");

    // Only needed when exactly 200 is required; any 2xx passes by default
    x.StatusCodeShouldBeOk();
});
```

`StatusCodeShouldBeSuccess()` restores the default success-range expectation after
`StatusCodeShouldBe(...)` or `IgnoreStatusCode()`, no longer registers a scenario assertion (so
it also works with `StreamServerSentEvents`), and moved from an extension method to an instance
method on `Scenario`. This also fixes
[#228](https://github.com/JasperFx/alba/issues/228), where `StatusCodeShouldBeSuccess()` ran the
default exact-200 assertion alongside the range check and failed 201/202/204 responses.

### `ReadAsXml` returns null on unparseable bodies instead of matching on "Error"

`IScenarioResult.ReadAsXml()` / `ReadAsXmlAsync()` previously returned `null` whenever the response
body merely *contained* the substring "Error", and threw on any other unparseable body. They now
attempt to parse and return `null` only when the body is not valid XML. Valid XML documents that
happen to contain the text "Error" now parse successfully.

## New features

- **Server-sent events support.** See the [docs page](https://github.com/JasperFx/alba/blob/master/docs/scenarios/sse.md)
- **Binary response reads.** `IScenarioResult.ReadAsBytes()` and `ReadAsBytesAsync()` return the
  response body as `byte[]`, for endpoints that return files, images or other payloads that
  `ReadAsText()` would corrupt. Like the other readers they leave the body readable, so a scenario
  can assert on the bytes and still call `ReadAsJson<T>()` afterwards.
- **HTTP QUERY support.** Scenarios can issue HTTP QUERY requests via `Scenario.Query`, matching
  the existing verb properties (`x.Query.Url("/api/query")`).
- **Time-travel testing with `TimeProviderOverride`.** A `FakeTimeProvider`-based extension that
  replaces the application's `TimeProvider` registration on every bootstrapping style. The
  extension is the clock: pass it to `AlbaHost.For(...)`, then drive time from the test with
  `Advance(...)` and `SetUtcNow(...)`
  ([#230](https://github.com/JasperFx/alba/issues/230)). Alba now depends on the
  `Microsoft.Extensions.TimeProvider.Testing` package.
- **Fluent host configuration.** Every bootstrapping method returns an awaitable `AlbaHostBuilder`
  with `WithConfiguration(...)` and `WithExtension(...)`, applied in the order they are chained on
  every bootstrapping style. On .NET 11, `AlbaHost.For<T>` applies
  configuration to the `WebApplicationBuilder` as soon as it is created, so values are readable in
  `Program.cs` before `builder.Build()` and reach applications that never forward `args` into
  `WebApplication.CreateBuilder()`
  ([#238](https://github.com/JasperFx/alba/issues/238)). The values no longer appear in the
  application's command line `args` on .NET 11.

## Improvements

- The response body is buffered once, asynchronously, immediately after each request completes.
  All response reads — including the synchronous `ReadAsText()`, `ReadAsJson<T>()`, and body
  assertions — are seekable, repeatable, memory-only operations.
- Scenario setup exceptions from asynchronous before-each actions surface directly with their
  original stack traces instead of being marshalled out of the test server callback.
- `FromHttpRequestMessage(...)` accepts every HTTP method. QUERY maps to `Scenario.Query`, and
  OPTIONS, TRACE, or any custom verb the application routes are applied directly instead of
  throwing `NotSupportedException`.
- A failure inside `IAlbaExtension.Start` now tears down the application that Alba already
  started instead of leaving an orphaned host and `TestServer` running for the rest of the test
  session. `IAlbaHost.DisposeAsync()` likewise completes every teardown step even when one fails,
  reporting the failures together as an `AggregateException`.

## Bug fixes

- Alba's MVC JSON strategy no longer selects third-party formatters (such as
  Microsoft.AspNetCore.OData's) that advertise `application/json` but cannot run outside their
  own pipeline. The framework's System.Text.Json or Newtonsoft.Json formatters are preferred
  regardless of registration order, fixing `PostJson`/`ReadAsJson` in applications that register
  OData ([#116](https://github.com/JasperFx/alba/issues/116)). Applications that intentionally
  front-loaded a custom JSON formatter alongside the framework one and relied on Alba picking
  the custom one now get the framework formatter; remove the framework JSON formatters if the
  custom one should win.
- Text and XML request bodies declare a `Content-Length` in bytes rather than characters.
  Non-ASCII bodies previously advertised a length shorter than the payload actually written.
- Scenario setup failures no longer execute the request. A `ConfigureHttpContext(...)` callback
  that throws, or a scenario with no url, used to run against the application with a half
  configured request before the exception surfaced. This applies to `StreamServerSentEvents`
  as well, and after-each actions now receive a null `HttpContext` for these failures.
- `PostJson(...)` serialized the request body twice. Custom `IJsonStrategy` implementations are
  now invoked once per scenario, matching `PutJson(...)`.
- OpenID Connect extensions dispose cleanly when they were never started. Disposing an extension
  whose `Start` failed validation threw a `NullReferenceException` that masked the real error.
- Configuration overrides on the `IHostBuilder` bootstrapping path (`AlbaHost.For(IHostBuilder)`,
  `StartAlbaAsync()`) now take precedence over the application's own configuration sources. They
  were added as host configuration, which `appsettings.json` and the other application sources
  overrode.
- `RedirectShouldBe(...)` and `RedirectPermanentShouldBe(...)` report a wrong status code once
  instead of twice.
- `MimeType` lookups are safe to use from multiple threads. `MimeTypeByValue(...)` and
  `MimeTypeByFileName(...)` cache misses into static state shared by every host.
