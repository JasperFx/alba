# Alba for AI Agents

This page is a ready-made prompt for AI coding agents (Claude Code, Copilot, Cursor, Codex and the like) that need
to add Alba integration tests to an ASP.NET Core project. It condenses the setup rules, the scenario API and the
common pitfalls into one block that you can paste into an agent's instructions, a `CLAUDE.md` / `AGENTS.md` file,
or the start of a conversation.

The complete documentation is also published in agent-friendly form at
[llms.txt](https://jasperfx.github.io/alba/llms.txt) (index) and
[llms-full.txt](https://jasperfx.github.io/alba/llms-full.txt) (every page in one file). Point an agent at those when it
needs more depth than the prompt below.

## The prompt

```md
You are adding Alba integration tests to an ASP.NET Core project. Alba (NuGet package `Alba`, version 9 or later)
runs HTTP requests through the real application in memory using ASP.NET Core's TestServer. There are no sockets and
no HttpClient. Follow these rules exactly; the API below is Alba 9 and differs from older samples you may have seen.

## Project setup
- .NET 10 or later is required. The test project and the application must target `net10.0` or `net11.0`.
- In the test project add a package reference to `Alba` and a project reference to the web project. Alba already
  brings `Microsoft.AspNetCore.Mvc.Testing`; do not add it separately.
- Keep the test framework the project already uses (xUnit, NUnit or TUnit). Do not switch frameworks.
- For a Minimal API / top-level statements application, the `Program` type is internal. Add
  `<InternalsVisibleTo Include="<TestProjectName>" />` to the web project's `.csproj` so `AlbaHost.For<Program>` compiles.

## Bootstrapping the host
`AlbaHost` boots the whole application and is expensive. Create it once and share it across tests.

    await using var host = await AlbaHost.For<Program>(x =>
        {
            // ASP.NET Core host customization goes in this action only
            x.UseEnvironment("Testing");
            x.ConfigureServices(s => s.AddSingleton<IExternalService, StubExternalService>());
        })
        // Alba specific setup is chained on the returned builder
        .WithConfiguration("ConnectionStrings:Db", "Host=localhost;Database=test")
        .WithExtension(new AuthenticationStub().WithName("test-user"));

- Every bootstrapping call returns an awaitable `AlbaHostBuilder`. The application starts when it is awaited.
  The builder only exposes `WithConfiguration(...)` and `WithExtension(...)` / `WithExtensions(...)`. Service
  registrations stay inside the `Action<IWebHostBuilder>` passed to `AlbaHost.For<T>`.
- `WithConfiguration` values override `appsettings.json`. On .NET 10 the application must call
  `WebApplication.CreateBuilder(args)` for them to flow through. Host-level keys like `environment` cannot be set this
  way; use `x.UseEnvironment(...)` instead.
- Extensions can also be passed as arguments: `AlbaHost.For<Program>(new AuthenticationStub())`.
- On .NET 11, `AlbaHost.For<T>` only supports applications built with `WebApplicationBuilder`. For a `Startup.cs`
  style application use `await AlbaHost.For(Program.CreateHostBuilder(args))` or
  `await Program.CreateHostBuilder(args).StartAlbaAsync()`. Both work on every target.
- Bootstrapping is asynchronous only. `new AlbaHost(...)`, `StartAlba()` and `ConfigurationOverride` do not exist
  in Alba 9.

## Sharing the host per test framework
- xUnit: a fixture class implementing `IAsyncLifetime` that awaits `AlbaHost.For<Program>()` in `InitializeAsync`
  and calls `DisposeAsync()` in `DisposeAsync`, consumed through `IClassFixture<T>` or a `[CollectionDefinition]` +
  `ICollectionFixture<T>` for sharing across classes. Test methods must be `async Task`.
- NUnit: a `[SetUpFixture]` class with a static `IAlbaHost Host` property set in `[OneTimeSetUp]` and disposed in
  `[OneTimeTearDown]`.
- TUnit: a class implementing `IAsyncInitializer` and `IAsyncDisposable` that owns the host, injected with
  `[ClassDataSource<AlbaBootstrap>(Shared = SharedType.PerTestSession)]`.
- Parallel tests that change data: share an `AlbaHostPool` (namespace `Alba.Pooling`) instead of one host. The factory builds each host for its
  slot, so hosts can use separate databases:
  `new AlbaHostPool(slot => AlbaHost.For<Program>().WithConfiguration("ConnectionStrings:Db", $"...Database=app_{slot.Index}"), new AlbaHostPoolOptions { MaxHosts = 4, OnReturn = (host, slot) => resetData(host) })`.
  Each test does `await using var host = await pool.RentAsync(cancellationToken);` and uses `host` like any
  `IAlbaHost`. Create per host extensions such as `HttpClientStub` inside the factory and find them with
  `host.GetExtension<HttpClientStub>()`. Rent one host per test, and always dispose the lease. To keep objects that
  are not in DI with each host (mocks, a DbContext, seeded users), use `AlbaHostPool<TState>`: the factory returns
  `(host, state)`, the options' actions receive `(host, state)`, and the test reads `lease.State`. Set
  `MaxConcurrentStarts = 1` when the application can't start several hosts at once.

## Writing scenarios
    var result = await host.Scenario(_ =>
    {
        _.Post.Json(new CreateItem("name")).ToUrl("/items");
        _.WithRequestHeader("x-tenant", "acme");
        _.StatusCodeShouldBe(201);
        _.Header("Location").SingleValueShouldMatch(new Regex(@"^/items/\d+$"));
        _.ContentShouldContain("name");
    });
    var item = await result.ReadAsJsonAsync<Item>();

- Verbs: `_.Get`, `_.Put`, `_.Post`, `_.Delete`, `_.Patch`, `_.Head`, `_.Query`, `_.Options`. Set the url with
  `.Url("/path")` for requests without a body, or `.Json(body).ToUrl("/path")` for requests with one.
- Request bodies: `.Json(obj)`, `.RawJson("{...}")`, `.Text("...")`, `.Xml(obj)`, `.FormData(obj)`,
  `_.WriteFormData(dictionary)`, `.ByteArray(bytes)`, `.Stream(stream)`. File uploads:
  `.MultipartFormData(form => form.File("upload", bytes, "report.csv").Field("tag", "q3"))`. Set headers with
  `_.WithRequestHeader(name, value)`, `.Accepts("...")`, `.ContentType("...")`.
- Query strings: `_.Get.Url("/search").QueryString("q", "alba")` or `.QueryString(new { q = "alba", page = 2 })`.
- Route names: when the endpoint has a route name (minimal API `.WithName(...)`, MVC `Name = ...`, Wolverine
  `RouteName`), prefer `_.Get.RouteNamed("GetAccount", new { accountId })` or
  `_.Post.Json(body).ToRouteNamed("MakeDeposit", new { accountId })` over a literal url. Extra values become the
  query string. An unknown name or missing value throws before the request and lists the known route names.
- Status codes: the default expectation is any 2xx. `_.StatusCodeShouldBeOk()` requires exactly 200,
  `_.StatusCodeShouldBe(404)` requires that code, `_.IgnoreStatusCode()` skips the check.
- Body assertions: `_.ContentShouldBe("exact")`, `_.ContentShouldContain("part")`, `_.ContentShouldNotContain("part")`,
  `_.ContentTypeShouldBe("application/json")`. Header assertions: `_.Header(name).SingleValueShouldEqual(...)`,
  `.ShouldHaveValues(...)`, `.ShouldNotBeWritten()`, `.SingleValueShouldMatch(regex)`.
  Redirects: `_.RedirectShouldBe("/url")`, `_.RedirectPermanentShouldBe("/url")`.
- JSON values without a DTO: `_.JsonPath("$.lines[0].sku").ShouldBe("A-1")`, `.ShouldExist()`, `.ShouldNotExist()`,
  `.ShouldHaveCount(2)`. Paths support `$`, `.name`, `['name']` and `[index]` only. `ShouldBe` compares structurally
  using the application's JSON settings; `_.JsonPath("$").ShouldBe(new { ... })` compares the whole body.
- Error responses: `_.ShouldReturnProblemDetails(409).WithTitle("...").WithDetail("...")` and
  `_.ShouldReturnValidationProblem().WithValidationError("Amount", "must be positive")` (field names ignore case,
  the message is a substring match).
- Failed assertions throw `ScenarioAssertionException` with the response body in the message. Assert on status and
  headers inside the scenario; assert on deserialized data after it returns.
- Reading the response: `result.ReadAsJsonAsync<T>()`, `ReadAsTextAsync()`, `ReadAsBytesAsync()`, `ReadAsXmlAsync()`,
  `ReadAsServerSentEvents<T>()`. Synchronous variants exist and every read is repeatable. `result.Context` is the raw
  `HttpContext`.
- JSON uses the application's own formatters (System.Text.Json or Newtonsoft). In an application that mixes Minimal
  API and MVC endpoints with customized JSON options, pass `JsonStyle.MinimalApi` or `JsonStyle.Mvc` to `.Json(...)`.
- Shorthands for pure JSON endpoints: `await host.GetAsJson<T>("/url")`, `await host.GetAsText("/url")`,
  `await host.PostJson(body, "/url").Receive<T>()`, `await host.PutJson(body, "/url").Receive<T>()`.
- Alba does not follow redirects, keep cookies between scenarios, or handle anti-forgery tokens. Each scenario is
  one independent request.

## Authentication
- `new AuthenticationStub().With("role", "admin").WithName("alice")` replaces every authentication scheme and
  authenticates each request with those claims. `new AuthenticationStub("SchemeName")` replaces only that scheme.
- `new JwtSecurityStub().With(...)` mints signed bearer tokens from the application's own `JwtBearerOptions` and
  disables OIDC discovery callouts.
- Per scenario: `_.WithClaim(new Claim("color", "green"))`, `_.RemoveClaim("role")`, `_.WithBearerToken(jwt)`.
- Against a real OIDC server: `OpenConnectClientCredentials` or `OpenConnectUserPassword` extensions.

## Hooks and time
- `host.BeforeEach(ctx => ...)` and `host.AfterEach(ctx => ...)` run synchronously around every request with the
  `HttpContext`. `host.BeforeEachAsync(scenario => ...)` runs before the request is built; modify the request with
  `scenario.WithRequestHeader(...)`, `scenario.WithBearerToken(...)` or `scenario.ConfigureHttpContext(...)`.
- `var clock = new TimeProviderOverride(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero));` passed to
  `AlbaHost.For<Program>(clock)` replaces the application's `TimeProvider`. Drive it with `clock.Advance(...)` and
  `clock.SetUtcNow(...)`.

## Outbound HTTP and endpoint checks
- Stub calls the application makes through `IHttpClientFactory` instead of reaching real services:
  `var payments = new HttpClientStub("payments").When(HttpMethod.Post, "/charges").RespondWithJson(charge);` passed
  to `AlbaHost.For<Program>(payments)`. A typed client's name is its class name; `new HttpClientStub()` stubs every
  client. Other responses: `RespondWith(HttpStatusCode.NotFound)`, `RespondWithText(...)`,
  `Throws(new HttpRequestException(...))`. The last matching rule wins. Assert on `payments.Requests` (each has
  `Uri`, `Headers`, `ReadAsJson<T>()`). Unmatched requests throw inside the application.
- `host.EndpointsAllowingAnonymous()` lists endpoints reachable without credentials, such as `"GET /health"`; assert
  the exact list to catch endpoints missing authorization. For coverage, pass one shared `EndpointCoverage` instance
  (a static field) to every host, then assert `coverage.NotExercised()` after the last test, in an xUnit assembly
  fixture, an NUnit `[SetUpFixture]` or a TUnit `[After(TestSession)]` hook.

## Pitfalls
- If the application does synchronous stream I/O and throws "Synchronous operations are disallowed", set
  `host.Server.AllowSynchronousIO = true` after the host starts.
- Snapshot testing: add Verify and Verify.AspNetCore, then `await Verify(result)` with the scenario result.
- Extend the DSL with extension methods on `Scenario` for repeated setup instead of copying it into every test.
- Real Docker dependencies such as a database or broker: if the solution has an Aspire AppHost, start it once per test
  assembly with `Aspire.Hosting.Testing`, remove the API resource, wait for the resources to be healthy, and pass their
  connection strings to the host with `WithConfiguration("ConnectionStrings:<name>", ...)`. See
  https://jasperfx.github.io/alba/guide/aspire.

## Deliverable
1. Add the package and project references, plus `InternalsVisibleTo` when needed.
2. Add one shared host fixture in the style of the project's test framework.
3. Write scenarios for the requested endpoints. Assert the status code and the response body in every scenario.
4. Run `dotnet test` and fix failures until the suite passes.
```

## Using it

- **Claude Code**: paste the block into the repository's `CLAUDE.md`, or into a project skill, so it loads with every session.
- **Other agents**: put it in `AGENTS.md`, `.cursor/rules`, or the equivalent instructions file for the tool.
- **One-off tasks**: paste it at the top of the task description together with the endpoints you want covered.

Keep the block in sync with the Alba version you reference. It describes Alba 9; the [v9 changelog](https://github.com/JasperFx/alba/blob/master/v9_CHANGELOG.md)
lists every API that changed from Alba 8.
