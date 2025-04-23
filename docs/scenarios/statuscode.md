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

        // Ignore the status code altogether
        _.IgnoreStatusCode();
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/StatusCodes.cs#L5-L20' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_check_the_status_code' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Do note that by default, if you do not specify the expected status code, Alba assumes that
the request should return 200 (OK) and will fail the scenario if a different status code is found. You
can ignore that check with the `Scenario.IgnoreStatusCode()` method.
