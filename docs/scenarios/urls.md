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
        _.Head.Url("/");
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Urls.cs#L5-L19' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_specify_the_url_directly' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

