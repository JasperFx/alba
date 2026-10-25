# Sending and Checking Json

JSON serialization is done with the configured input and output formatters within the underlying application. This means that
Alba can support systems using both System.Text.Json and Newtonsoft.Json. Alba picks the application's System.Text.Json or
Newtonsoft.Json formatters even when other registered formatters (such as ASP.NET Core OData's) also advertise
`application/json`, so the JSON helpers work in applications that mix OData and plain API controllers.

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

## Sending Raw Json

When you already have the JSON payload as a string — malformed-input tests, captured payloads,
and the like — skip serialization and write it directly:

```cs
await host.Scenario(_ =>
{
    _.Post.RawJson("{\"name\": \"Max\", \"age\": 13}").ToUrl("/person");
});
```

This writes the string to the request body verbatim and sets the `content-type` header to
`application/json`.


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

## Asserting on JSON Structure

`JsonPath(...)` asserts on one value in a JSON response body, without using a DTO. Expected values are
serialized with the application's own JSON settings, so enums, naming policies and converters match what the
application writes:

<!-- snippet: sample_json_path_assertions -->
<a id='snippet-sample_json_path_assertions'></a>
```cs
[Fact]
public async Task values_at_paths()
{
    await _host.Scenario(x =>
    {
        x.Get.Url("/order");
        x.JsonPath("$.id").ShouldBe(7);
        x.JsonPath("$.customer.name").ShouldBe("Alba");
        x.JsonPath("$.lines[1].sku").ShouldBe("B-2");
        x.JsonPath("$['customer']['name']").ShouldBe("Alba");

        // The leading "$." is optional
        x.JsonPath("lines[0].quantity").ShouldBe(2);
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Acceptance/json_path_assertions.cs#L55-L71' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_json_path_assertions' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Paths use a small JSONPath subset: `$` for the root, `.name` or `['name']` for a property, and `[0]` for an array
item. The leading `$.` can be left out. Filters, wildcards and recursive descent are not supported.

`ShouldBe(...)` compares structurally: property order and whitespace do not matter, and numbers compare by value, so
`150` equals `150.0`. Objects, anonymous types and collections compare the same way, and the path `$` compares the
whole body:

<!-- snippet: sample_json_path_structural_comparison -->
<a id='snippet-sample_json_path_structural_comparison'></a>
```cs
[Fact]
public async Task objects_compare_structurally_regardless_of_property_order()
{
    await _host.Scenario(x =>
    {
        x.Get.Url("/order");
        x.JsonPath("$.lines[0]").ShouldBe(new { quantity = 2, sku = "A-1" });
        x.JsonPath("$.customer").ShouldBe(new { email = (string)null, name = "Alba" });
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Acceptance/json_path_assertions.cs#L73-L84' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_json_path_structural_comparison' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

`ShouldExist()`, `ShouldNotExist()` and `ShouldHaveCount(...)` check that a member is present or absent, or that an
array has a given length. A member whose value is JSON `null` counts as present:

<!-- snippet: sample_json_path_existence_and_counts -->
<a id='snippet-sample_json_path_existence_and_counts'></a>
```cs
[Fact]
public async Task existence_and_counts()
{
    await _host.Scenario(x =>
    {
        x.Get.Url("/order");
        x.JsonPath("$.customer.email").ShouldExist().ShouldBe<string>(null);
        x.JsonPath("$.customer.phone").ShouldNotExist();
        x.JsonPath("$.lines[2]").ShouldNotExist();
        x.JsonPath("$.lines").ShouldHaveCount(2);
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Acceptance/json_path_assertions.cs#L130-L143' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_json_path_existence_and_counts' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

As with the other scenario assertions, every failed path is reported together with the response body. For error
responses in the problem details format, see [Problem Details](problemdetails.md).

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
