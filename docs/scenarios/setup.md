# Before and After actions

::: warning
The Before/After actions are **not** additive. The last one specified is the only one executed.
:::

Alba allows you to specify actions that run immediately before or after an HTTP request is executed for common setup or teardown
work like setting up authentication credentials or tracing or whatever.

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

// Asynchronously
system.BeforeEachAsync(context =>
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
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/before_and_after_actions.cs#L30-L59' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_before_and_after' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
