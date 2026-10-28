---
title: Specifying Request Urls
description: Set the request url, HTTP method, query string and route name values in Alba scenarios.
---

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
        _.Options.Url("/");
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Urls.cs#L5-L22' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_specify_the_url_directly' title='Start of snippet'>anchor</a></sup>
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
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Urls.cs#L24-L37' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_query_string_parameters' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Route names

Instead of repeating a url that the endpoint already owns, a scenario can address an endpoint by its
ASP.NET Core route name. Alba generates the url from the running application's routing, so the test keeps working
when the route pattern changes:

<!-- snippet: sample_route_names -->
<a id='snippet-sample_route_names'></a>
```cs
public async Task route_names(IAlbaHost system)
{
    var accountId = 42;

    await system.Scenario(_ =>
    {
        // Route values fill the route parameters of the named endpoint,
        // and the rest become the query string
        _.Get.RouteNamed("GetAccount", new { accountId, include = "deposits" });
    });

    await system.Scenario(_ =>
    {
        // With a request body, name the route after the body
        _.Post.Json(new { Amount = 100m }).ToRouteNamed("MakeDeposit", new { accountId });
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Urls.cs#L39-L57' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_route_names' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Route names come from minimal API `.WithName(...)`, MVC attribute routes with `Name = ...`, and Wolverine's
`RouteName` endpoint property. Route values can be an anonymous object, a dictionary, or a `RouteValueDictionary`.
Values that are not route parameters become the query string, and `QueryString(...)` can add more after the url
is generated. Route name lookup ignores case.

An unknown route name, a missing required route value, or a value that fails a route constraint throws an
`InvalidOperationException` before the request is sent. The message lists the route names the application knows, or the
route pattern and the values that are missing, instead of letting the scenario fail with a 404.
