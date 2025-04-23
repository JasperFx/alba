# Sending and Checking Xml

I wasn't sure that Xml over HTTP was all that common of a use case anymore, but the very first project we
used Alba on turned out to be an Xml API, so here we are.

In all cases, Alba just uses the old, built in `XmlSerializer` from the .Net BCL.

## Sending Xml in the Request

There's a similar helper for Xml serialization that allows you to send an object
as xml to the web request:

<!-- snippet: sample_sending_xml -->
<a id='snippet-sample_sending_xml'></a>
```cs
public async Task send_xml(IAlbaHost host)
{
    await host.Scenario(_ =>
    {
        // This serializes the Input object to xml,
        // writes it to the HttpRequest.Body, and sets
        // the accepts & content-type header values to
        // application/xml
        _.Post
            .Xml(new Input {Name = "Max", Age = 13})
            .ToUrl("/person");

        // OR, if url lookup is enabled, this is an equivalent:
        _.Post.Xml(new Input {Name = "Max", Age = 13});
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/JsonAndXml.cs#L37-L54' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_sending_xml' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Reading Xml from the Response

There's a helper off of the `HttpResponseBody` for reading Xml from the response:

<!-- snippet: sample_read_xml -->
<a id='snippet-sample_read_xml'></a>
```cs
public async Task read_xml(IAlbaHost host)
{
    var result = await host.Scenario(_ =>
    {
        _.Get.Url("/output");
    });

    // This deserializes the response body to the
    // designated Output type
    var output = result.ReadAsXml<Output>();

    // do assertions against the Output model

    // OR, if you just want the XmlDocument itself:
    XmlDocument document = await result.ReadAsXmlAsync();
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/JsonAndXml.cs#L125-L142' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_read_xml' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
