# Working with Plain Text Requests

If you find yourself needing to test HTTP endpoints that either send text or return text, Alba
has you covered with some built in helpers.

## Reading the Response Text

To read the response body as text, use this syntax:

<!-- snippet: sample_read_text -->
<a id='snippet-sample_read_text'></a>
```cs
public async Task read_text(IAlbaHost host)
{
    var result = await host.Scenario(_ =>
    {
        _.Get.Url("/output");
    });

    // This deserializes the response body to the
    // designated Output type
    var outputString = await result.ReadAsTextAsync();

    // do assertions against the Output string
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/JsonAndXml.cs#L82-L96' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_read_text' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Assertions against the Response Text

You have these built in operations for asserting on the response body text:

<!-- snippet: sample_assert_on_text -->
<a id='snippet-sample_assert_on_text'></a>
```cs
public async Task assert_on_content(IAlbaHost host)
{
    await host.Scenario(_ =>
    {
        _.ContentShouldBe("exactly this");

        _.ContentShouldContain("some snippet");

        _.ContentShouldNotContain("some warning");
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/JsonAndXml.cs#L98-L110' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_assert_on_text' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Sending Text

Lastly, you can send text to an HTTP endpoint with this syntax:

<!-- snippet: sample_send_text -->
<a id='snippet-sample_send_text'></a>
```cs
public async Task send_text(IAlbaHost host)
{
    await host.Scenario(_ =>
    {
        _.Post.Text("some text").ToUrl("/textdata");
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/JsonAndXml.cs#L113-L121' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_send_text' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Do note that this also sets the `content-length` header to the string length and
sets the `content-type` header of the request to "text/plain."
