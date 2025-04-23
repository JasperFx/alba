# Sending and Checking Json

JSON serialization is done with the configured input and output formatters within the underlying application. This means that
Alba can support systems using both System.Text.Json and Newtonsoft.Json.

## Sending Json

::: tip
Minimal API and MVC Core endpoints do JSON serialization a little bit differently. When testing
a system that has mixed Minimal API and MVC Core endpoints **and the JSON serialization has been customized
within the application**, you may need to help Alba out
a little bit to know what type of route you're testing.
:::

Since posting Json to a web server API is so common, Alba has some helpers for writing Json to the request:

<!-- snippet: sample_sending_json -->
<a id='snippet-sample_sending_json'></a>
```cs
public async Task send_json(IAlbaHost host)
{
    await host.Scenario(_ =>
    {
        // This serializes the Input object to json,
        // writes it to the HttpRequest.Body, and sets
        // the accepts & content-type header values to
        // application/json
        _.Post
            .Json(new Input {Name = "Max", Age = 13})
            .ToUrl("/person");
    });
}

public async Task send_json_minimal_api(IAlbaHost host)
{
    await host.Scenario(_ =>
    {
        // In a system that has mixed Minimal API and MVC usage,
        // you may need to help Alba know if the route being tested
        // should use Minimal API or MVC Core compliant JSON testing
        _.Post
            .Json(new Input {Name = "Max", Age = 13}, JsonStyle.MinimalApi)
            .ToUrl("/person");
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/JsonAndXml.cs#L7-L34' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_sending_json' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Reading Json

To deserialize the response body with Json to interrogate the results in a strong typed way, use this syntax:

<!-- snippet: sample_read_json -->
<a id='snippet-sample_read_json'></a>
```cs
public async Task read_json(IAlbaHost host)
{
    var result = await host.Scenario(_ =>
    {
        _.Get.Url("/output");
    });

    // This deserializes the response body to the
    // designated Output type
    var output = result.ReadAsJsonAsync<Output>();

    // do assertions against the Output model
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/JsonAndXml.cs#L56-L70' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_read_json' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

You can also use a shorthand syntax to skip `Scenario()` like this:

<!-- snippet: sample_read_json_shorthand -->
<a id='snippet-sample_read_json_shorthand'></a>
```cs
public async Task read_json_shorthand(IAlbaHost host)
{
    var output = await host.GetAsJson<Output>("/output");

    // do assertions against the Output model
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/JsonAndXml.cs#L72-L79' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_read_json_shorthand' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

This code snippet is functionally identical to the previous usage.

## Shortcut for Pure Json Services

If you don't care about any additional HTTP headers or need to verify any HTTP status code except for `200 Ok`, you can use this shorthand syntax
to quickly post and receive results from a web service:

<!-- snippet: sample_post_json_get_json -->
<a id='snippet-sample_post_json_get_json'></a>
```cs
[Fact]
public async Task post_and_expect_response()
{
    await using var system = await AlbaHost.For<WebApp.Program>();
    var request = new OperationRequest
    {
        Type = OperationType.Multiply,
        One = 3,
        Two = 4
    };

    var result = await system.PostJson(request, "/math")
        .Receive<OperationResult>();
        
    result.Answer.ShouldBe(12);
    result.Method.ShouldBe("POST");
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/using_json_helpers.cs#L21-L39' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_post_json_get_json' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

There are similar helpers for other HTTP verbs like `PUT` and `DELETE`.
