# Working with Urls

The simplest way to specify the url for the request is to use one of these calls shown below,
depending upon the HTTP method:

<!-- snippet: sample_specify_the_url_directly -->
<a id='snippet-sample_specify_the_url_directly'></a>
```cs
public async Task specify_url(AlbaHost system)
{
    await system.Scenario(_ =>
    {
        // Directly specify the Url against a given
        // HTTP method
        _.Get.Url("/");
        _.Put.Url("/");
        _.Post.Url("/");
        _.Delete.Url("/");
        _.Patch.Url("/");
        _.Head.Url("/");
        _.Query.Url("/");
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Urls.cs#L5-L21' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_specify_the_url_directly' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Query string parameters

Query string parameters can be appended individually or from the public properties and fields
of an object:

<!-- snippet: sample_query_string_parameters -->
<a id='snippet-sample_query_string_parameters'></a>
```cs
public async Task query_string_parameters(AlbaHost system)
{
    await system.Scenario(_ =>
    {
        // Add individual query string parameters
        _.Get.Url("/search").QueryString("q", "alba").QueryString("page", "2");

        // Or append one parameter per public property or field
        // of an object
        _.Get.Url("/search").QueryString(new { q = "alba", page = 2 });
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Urls.cs#L23-L36' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_query_string_parameters' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

