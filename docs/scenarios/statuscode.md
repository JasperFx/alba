# Http Status Codes

You can declaratively check the status code with this syntax:

<!-- snippet: sample_check_the_status_code -->
<a id='snippet-sample_check_the_status_code'></a>
```cs
public async Task check_the_status(IAlbaHost system)
{
    await system.Scenario(_ =>
    {
        // Shorthand for saying that the StatusCode should be 200
        _.StatusCodeShouldBeOk();

        // Or a specific status code
        _.StatusCodeShouldBe(403);

        // Any status code between 200 and 299; this is the
        // default expectation for every scenario
        _.StatusCodeShouldBeSuccess();

        // Ignore the status code altogether
        _.IgnoreStatusCode();
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/StatusCodes.cs#L5-L24' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_check_the_status_code' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Do note that by default, if you do not specify the expected status code, Alba assumes that
the request should return a success status code (200-299) and will fail the scenario otherwise.
Use `StatusCodeShouldBe(...)` or `StatusCodeShouldBeOk()` to require one exact status code, or
`Scenario.IgnoreStatusCode()` to skip the check altogether. `StatusCodeShouldBeSuccess()`
restores the default success-range expectation after either of those.
