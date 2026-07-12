# Before and After actions

Alba lets you register actions that run immediately before or after each scenario's HTTP request for common setup or teardown
work like setting up authentication credentials or tracing. Registrations are additive — every registered action runs for every scenario.

There are two kinds of *before* actions:

- `BeforeEachAsync(Func<Scenario, Task>)` — asynchronous preparation that runs **before** the HTTP request is created. It receives the
  `Scenario`, so it can do real asynchronous work (fetch a token, hit a database) and then modify the outgoing request through
  `scenario.ConfigureHttpContext(...)` or helpers like `scenario.WithRequestHeader(...)` and `scenario.WithBearerToken(...)`.
- `BeforeEach(Action<HttpContext>)` — synchronous work that runs against the live `HttpContext` immediately before the request executes.

All asynchronous prepare actions run first (in registration order), then all synchronous actions (in registration order).

Here's a sample:

<!-- snippet: sample_before_and_after -->
<a id='snippet-sample_before_and_after'></a>
```cs
// Synchronously
system.BeforeEach(context =>
{
    // Modify the HttpContext immediately before each
    // Scenario()/HTTP request is executed
    context.Request.Headers.Append("trace", "something");
});

system.AfterEach(context =>
{
    // perform an action immediately after the scenario/HTTP request
    // is executed
});

// Asynchronously, before the HTTP request executes. Modify the
// outgoing request itself through scenario.ConfigureHttpContext()
system.BeforeEachAsync(scenario =>
{
    // do something asynchronous here
    return Task.CompletedTask;
});

system.AfterEachAsync(context =>
{
    // do something asynchronous here
    return Task.CompletedTask;
});
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/before_and_after_actions.cs#L30-L60' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_before_and_after' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
