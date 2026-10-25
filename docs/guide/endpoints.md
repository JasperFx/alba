# Endpoint Coverage and Authorization

Alba can report which of the application's endpoints the test suite reaches, and which ones a request without
credentials can hit. Both checks describe endpoints by their HTTP methods and route pattern, such as
`"GET /accounts/{id:int}"`.

## Endpoints that no test reaches

The `EndpointCoverage` extension, in the `Alba.Extensions` namespace, records each endpoint that ASP.NET Core routes a
request to. Pass one instance to
every host of the application, then call `NotExercised()` after the last test has run to list the endpoints that no
request reached:

<!-- snippet: sample_endpoint_coverage_fixture -->
<a id='snippet-sample_endpoint_coverage_fixture'></a>
```cs
public class ApiCoverage : IAsyncLifetime
{
    // One instance for every host of the application, however many fixtures or pools create them
    public static readonly EndpointCoverage Coverage = new();

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
    {
        // As an assembly fixture, this runs after the last test in the assembly.
        // The exception fails the test run, naming each endpoint that no test reached.
        Coverage.NotExercised().ShouldBeEmpty();
        return ValueTask.CompletedTask;
    }
}

public class ApiFixture : IAsyncLifetime
{
    public IAlbaHost Host = null!;

    public async ValueTask InitializeAsync()
    {
        Host = await AlbaHost.For<WebApp.Program>(ApiCoverage.Coverage);
    }

    public async ValueTask DisposeAsync()
    {
        await Host.DisposeAsync();
    }
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/EndpointAudits.cs#L6-L37' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_endpoint_coverage_fixture' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Register the coverage class as an assembly fixture, so that its `DisposeAsync` runs after every test in the assembly:

```cs
[assembly: AssemblyFixture<ApiCoverage>]
```

The exception from `ShouldBeEmpty()` fails the test run. To report the gaps without failing the run, write them to the
test output instead. With NUnit, check the coverage in a `[OneTimeTearDown]` method of a `[SetUpFixture]`; with TUnit,
check it in an `[After(TestSession)]` hook.

- One instance can be shared by any number of hosts, such as every host of an [`AlbaHostPool`](pooling.md), and it
  keeps its results after the hosts are disposed. Hosts that are not given the instance are not counted.
- An endpoint counts as soon as routing selects it, whatever the response, so a test that expects a 401 from an existing
  endpoint covers it. Requests to urls that match no endpoint do not count.
- Every request through the host counts: scenarios, server-sent event streams, and clients from
  `host.Server.CreateClient()`.
- Coverage is recorded per test process. Each test project and each target framework runs in its own process and
  reports on its own.

## Endpoints that allow anonymous requests

`EndpointsAllowingAnonymous()` lists the endpoints that a request without credentials can reach. These are the
endpoints with `[AllowAnonymous]` or `AllowAnonymous()`. When the application has no fallback authorization policy,
they also include every endpoint with no authorization requirement. The rules match ASP.NET Core's authorization
middleware: `[AllowAnonymous]` wins over `[Authorize]`, and a fallback policy protects every endpoint that has no
authorization metadata.

Asserting on the list makes a cheap security regression test. A new endpoint that is missing `[Authorize]` or
`RequireAuthorization()` changes the list and fails the test:

<!-- snippet: sample_endpoints_allowing_anonymous -->
<a id='snippet-sample_endpoints_allowing_anonymous'></a>
```cs
[Fact]
public async Task no_endpoint_allows_anonymous_requests()
{
    await using var host = await AlbaHost.For<WebAppSecuredWithJwt.Program>();

    // A new endpoint that forgets [Authorize] or RequireAuthorization() fails this test
    host.EndpointsAllowingAnonymous().ShouldBeEmpty();
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/EndpointAudits.cs#L41-L50' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_endpoints_allowing_anonymous' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

For an application with public endpoints, assert the exact list instead, such as
`ShouldBe(["GET /health", "POST /login"])`.

Only endpoint metadata is examined. This check cannot see authorization that MVC applies through a global
`AuthorizeFilter`, or that the application performs in its own code.
