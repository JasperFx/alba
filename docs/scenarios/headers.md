# Working with Headers

We really didn't worry much about HTTP niceties when I started web programming in the late 90's, but you
can no longer get away with that in today's world. To more correctly use HTTP, Alba comes with some helpers
to deal with HTTP header values.

## Setting Request Headers

To set request headers, you can directly write against the `HttpContext.Request.Headers` collection:

<!-- snippet: sample_setting_request_headers -->
<a id='snippet-sample_setting_request_headers'></a>
```cs
public async Task setting_request_headers(IAlbaHost system)
{
    await system.Scenario(_ =>
    {
        _.WithRequestHeader("foo", "bar");
        
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Headers.cs#L29-L38' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_setting_request_headers' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

There are also some specific helpers for very common [content negotiation-related](https://en.wikipedia.org/wiki/Content_negotiation) headers:

<!-- snippet: sample_conneg_helpers -->
<a id='snippet-sample_conneg_helpers'></a>
```cs
public async Task conneg_helpers(IAlbaHost system)
{
    await system.Scenario(_ =>
    {
        // Set the accepts header on the request
        _.Get.Url("/").Accepts("text/plain");

        // Specify the etag header value
        _.Get.Url("/").Etag("12345");

        // Set the content-type header on the request
        _.Post.Url("/").ContentType("text/json");

        // This is a superset of the code above that
        // will set the content-type header as well
        _.Post.Json(new InputModel()).ToUrl("/");
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Headers.cs#L8-L27' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_conneg_helpers' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

**Do add extension methods off of the Alba `Scenario` class for common operations in your tests to remove
some of the tedium.**

## Asserting on Expected Response Headers

Alba comes with some out of the box assertions to declaratively check expected header values:

<!-- snippet: sample_asserting_on_header_values -->
<a id='snippet-sample_asserting_on_header_values'></a>
```cs
public async Task asserting_on_header_values(IAlbaHost system)
{
    await system.Scenario(_ =>
    {
        // Assert that there is one and only one value equal to "150"
        _.Header(HeaderNames.ContentLength).SingleValueShouldEqual("150");

        // Assert that there is no value for this response header
        _.Header(HeaderNames.Connection).ShouldNotBeWritten();

        // Only write one value for this header
        _.Header(HeaderNames.SetCookie).ShouldHaveOneNonNullValue();

        // Assert that the header has any values
        _.Header(HeaderNames.ETag).ShouldHaveValues();

        // Assert that the header has the given values
        _.Header(HeaderNames.WWWAuthenticate).ShouldHaveValues("NTLM", "Negotiate");

        // Assert that the header matches a regular expression
        _.Header(HeaderNames.Location).SingleValueShouldMatch(new Regex(@"^/items/\d*$"));

        // Check the content-type header
        _.ContentTypeShouldBe("text/json");
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Headers.cs#L41-L68' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_asserting_on_header_values' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

You do also have the ability to just interrogate the `HttpContext.Response` in your unit test methods for
anything not covered in the helpers above. 
