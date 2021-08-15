<!--title:Sending and Checking Xml-->

I wasn't sure that Xml over HTTP was all that common of a use case anymore, but the very first project we
used Alba on turned out to be an Xml API, so here we are.

In all cases, Alba just uses the old, built in `XmlSerializer` from the .Net BCL.

## Sending Xml in the Request

There's a similar helper for Xml serialization that allows you to send an object
as xml to the web request:

snippet: sample_sending_xml


## Reading Xml from the Response

There's a helper off of the `HttpResponseBody` for reading Xml from the response:

snippet: sample_read_xml
