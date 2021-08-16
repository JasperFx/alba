---
title:Sending and Checking Json
editLink:true
---

## Sending Json

Since posting Json to a web server API is so common, Alba has some helpers for writing Json to the request:

<!-- snippet: sample_sending_json -->
<a id='snippet-sample_sending_json'></a>
```cs
public Task send_json(IAlbaHost system)
{
    return system.Scenario(_ =>
    {
        // This serializes the Input object to json,
        // writes it to the HttpRequest.Body, and sets
        // the accepts & content-type header values to
        // application/json
        _.Post
            .Json(new Input {Name = "Max", Age = 13})
            .ToUrl("/person");

        // OR, if url lookup is enabled, this is an equivalent:
        _.Post.Json(new Input {Name = "Max", Age = 13});
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/JsonAndXml.cs#L10-L27' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_sending_json' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Reading Json

To deserialize the response body with Json to interrogate the results in a strong typed way, use this syntax:

<!-- snippet: sample_read_json -->
<a id='snippet-sample_read_json'></a>
```cs
public async Task read_json(IAlbaHost system)
{
    var result = await system.Scenario(_ =>
    {
        _.Get.Url("/output");
    });

    // This deserializes the response body to the
    // designated Output type
    var output = result.ReadAsJson<Output>();

    // do assertions against the Output model
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/JsonAndXml.cs#L48-L62' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_read_json' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
