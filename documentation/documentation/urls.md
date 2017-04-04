<!--title: Working with Url's-->

The simplest way to specify the url for the request is to use one of these calls shown below,
depending upon the HTTP method:

<[sample:specify-the-url-directly]>

**If** your system under test supports it, you can use reverse url lookup to specify the Url's in terms of your application code:

<[sample:reverse-url-lookup]>

This mechanism has been helpful in our earlier usage of the [FubuMVC Scenario testing](https://jeremydmiller.com/2015/11/05/testing-http-handlers-with-no-web-server-in-sight/) that Alba is based on to create traceability from
the tests to the underlying code.

<div class="alert alert-warning"><b>Note!</b> Alba does not yet have a url lookup strategy for MVC applications, but one is planned. These methods will throw NotSupportedException's out of the box.</div>

To actually implement the reverse url lookup, you can create your own implementation of the `IUrlLookup` interface:

<[sample:IUrlLookup]>

and set the `SystemUnderTest.Urls` property to your new lookup strategy.

There are some specific Url helpers for sending Json or Xml data as well. See also:

* <[linkto:documentation/json]>
* <[linkto:documentation/xml]>