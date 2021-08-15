---
title:Getting Started
editLink:true
---

::: warning
As of 5.0+, Alba only supports > .Net 5.0 applications. We have dropped all support for any version of ASP.NET Core before 3.1. Use Alba 3.x if you still require 2.1 support.
:::

Alba is a class library that you use in combination with unit testing tools like [xUnit.Net](https://xunit.github.io) or [NUnit](https://docs.nunit.org/) to author integration tests
against ASP.NET Core HTTP endpoints. Alba *scenarios* actually exercise the full ASP.Net Core application by running HTTP requests through your ASP.NET system **in memory** using the 
built in [ASP.Net Core TestServer](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-5.0).

To get started with Alba, just add a Nuget reference to the Alba library to your testing project. In the next section I'll show you how to bootstrap
your ASP.Net Core system with Alba and start authoring specifications with the `IAlbaHost` abstraction.

::: tip
Do note that Alba is not directly coupled to xUnit and would be usable within any .Net unit testing library.
:::


## Initializing AlbaHost

To bootstrap and connect any ASP.Net Core application to Alba, create a `AlbaHost` using the definition of your `IHostBuilder` as shown below:

<!-- snippet: sample_Quickstart3 -->
<a id='snippet-sample_quickstart3'></a>
```cs
[Fact]
public async Task build_host_from_Program()    
{
    // Bootstrap your application just as your real application does
    var hostBuilder = Program.CreateHostBuilder(Array.Empty<string>());

    await using var system = new AlbaHost(hostBuilder);
    
    // Just as a sample, I'll run a scenario against
    // a "hello, world" application's root url
    await system.Scenario(s =>
    {
        s.Get.Url("/");
        s.ContentShouldBe("Hello world.");
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Quickstart3.cs#L15-L32' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_quickstart3' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The `AlbaHost` is an extension of the standard .Net Core [IHost](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.ihost?view=dotnet-plat-ext-5.0) interface with a few additions for testing support.
While you can always access the underlying `TestServer` through the `IAlbaHost.Server` property, you're mostly going to be using the `Scenario()` method to write Alba "Scenario" tests.


## Writing your first scenario

::: tip
Alba is not directly coupled to MVC Core in any way and executes requests through your application without any knowledge of the middleware,
controllers, or the other mechanisms that may be handling the request in your application.
:::

For the purpose of this sample, let's say you have a very simple web service application with the following controller endpoint:


<!-- snippet: sample_MathController -->
<a id='snippet-sample_mathcontroller'></a>
```cs
public enum OperationType
{
    Add,
    Subtract,
    Multiply,
    Divide
}

public class OperationRequest
{
    public OperationType Type { get; set; }
    public int One { get; set; }
    public int Two { get; set; }
}

public class OperationResult
{
    public int Answer { get; set; }
    public string Method { get; set; }
}

[ApiController]
[Route("[controller]")]
public class MathController : Controller
{
    [HttpGet("add/{one}/{two}")]
    public OperationResult Add(int one, int two)
    {
        return new OperationResult
        {
            Answer = one + two
        };
    }

    [HttpPut]
    public OperationResult Put([FromBody]OperationRequest request)
    {
        switch (request.Type)
        {
            case OperationType.Add:
                return new OperationResult{Answer = request.One + request.Two, Method = "PUT"};
            
            case OperationType.Multiply:
                return new OperationResult{Answer = request.One * request.Two, Method = "PUT"};
            
            case OperationType.Subtract:
                return new OperationResult{Answer = request.One - request.Two, Method = "PUT"};
            
            default:
                throw new ArgumentOutOfRangeException(nameof(request.Type));
        }
    }
    
    [HttpPost]
    public OperationResult Post([FromBody]OperationRequest request)
    {
        switch (request.Type)
        {
            case OperationType.Add:
                return new OperationResult{Answer = request.One + request.Two, Method = "POST"};
                
            case OperationType.Multiply:
                return new OperationResult{Answer = request.One * request.Two, Method = "POST"};
            
            case OperationType.Subtract:
                return new OperationResult{Answer = request.One - request.Two, Method = "POST"};
            
            default:
                throw new ArgumentOutOfRangeException(nameof(request.Type));
        }
    }
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/WebApp/Controllers/MathController.cs#L6-L79' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_mathcontroller' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Back in your test project, the easiest, and probably most common, usage of Alba is to send and verify JSON message bodies to `Controller` actions. To that end, 
let's test the GET method in that controller above by passing a url and verifying the results:

<!-- snippet: sample_get_json -->
<a id='snippet-sample_get_json'></a>
```cs
[Fact]
public async Task get_happy_path()
{
    var builder = Program.CreateHostBuilder(Array.Empty<string>());

    await using var system = new AlbaHost(builder);
    
    // Issue a request, and check the results
    var result = await system.GetAsJson<OperationResult>("/math/add/3/4");
        
    result.Answer.ShouldBe(7);
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/using_json_helpers.cs#L21-L34' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_get_json' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

So what just happened in that test? First off, the call to `new AlbaHost(IHostBuilder)` bootstraps your web application. 

The call to `host.GetAsJson<OperationResult>("/math/add/3/4")` is performing these steps internally:

1. Formulate an [HttpRequest](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httprequest?view=aspnetcore-5.0) object that will be passed to the application
1. Executes the web request against your application
1. Assert in this simple use case that the response status code is `200 OK`
1. Read the raw JSON coming off the [HttpResponse](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpresponse?view=aspnetcore-5.0)
1. Deserialize the raw JSON to the requested `OperationResult` type using the Json formatter of the running application
1. Returns the resulting `OperationResult`

Alright then, let's try posting JSON in and examining the JSON out:

<!-- snippet: sample_post_json_get_json -->
<a id='snippet-sample_post_json_get_json'></a>
```cs
[Fact]
public async Task post_and_expect_response()
{
    using var system = AlbaHost.ForStartup<WebApp.Startup>();
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
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/using_json_helpers.cs#L36-L54' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_post_json_get_json' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

It's a little more complicated, but the same goal is realized here. Allow the test author to work in terms of the application model objects while still exercising the entire HTTP middleware stack.

Don't stop here though, Alba also gives you the ability to declaratively assert on elements of the `HttpResponse` like expected header values, status codes, and assertions against the response body. In addition, Alba provides a lot of helper facilities to work with the raw `HttpResponse` data.

::: tip
As of Alba V5, the scenario support is no longer hard coded to use Newtonsoft.Json for Json serialization and will instead use the configured
Json formatters within your application. Long story short, Alba now supports applications using System.Text.Json as well as Newtonsoft.Json. 
:::

## Testing Hello, World

Now let's say that you built the obligatory hello world application for ASP.Net Core shown below:

<!-- snippet: sample_HelloWorldApp -->
<a id='snippet-sample_helloworldapp'></a>
```cs
public class Startup
{
    public void Configure(IApplicationBuilder builder)
    {
        builder.Run(context =>
        {
            context.Response.Headers["content-type"] = "text/plain";
            return context.Response.WriteAsync("Hello, World!");
        });
    }
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Quickstart.cs#L215-L227' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_helloworldapp' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

We can now use Alba to declare an integration test for our Hello, World application within an [xUnit](http://xunit.github.io/)
testing project:

<!-- snippet: sample_should_say_hello_world -->
<a id='snippet-sample_should_say_hello_world'></a>
```cs
[Fact]
public async Task should_say_hello_world()
{
    await using var host = await Program
        .CreateHostBuilder(Array.Empty<string>())
        
        // This extension method is just a shorter version
        // of new AlbaHost(builder)
        .StartAlbaAsync();
    
    // This runs an HTTP request and makes an assertion
    // about the expected content of the response
    await host.Scenario(_ =>
    {
        _.Get.Url("/");
        _.ContentShouldBe("Hello, World!");
        _.StatusCodeShouldBeOk();
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Quickstart.cs#L30-L50' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_should_say_hello_world' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The sample up above bootstraps the application defined by our `Startup` and executes a *Scenario* against the running system.
A *scenario* in Alba defines how the HTTP request should be constructed (the request body, headers, url) and optionally gives you
the ability to express assertions against the expected HTTP response.

Alba comes with plenty of helpers in its [fluent interface](https://www.martinfowler.com/bliki/FluentInterface.html) to work with the `HttpRequest` and `HttpResponse`, or you can work directly with the underlying ASP.Net Core objects:

<!-- snippet: sample_should_say_hello_world_with_raw_objects -->
<a id='snippet-sample_should_say_hello_world_with_raw_objects'></a>
```cs
[Fact]
public async Task should_say_hello_world_with_raw_objects()
{
    using (var system = AlbaHost.ForStartup<Startup>())
    {
        var response = await system.Scenario(_ =>
        {
            _.Get.Url("/");
            _.StatusCodeShouldBeOk();
        });

        response.ReadAsText()
            .ShouldBe("Hello, World!");

        // or you can go straight at the HttpContext
        Stream responseStream = response.Context.Response.Body;
        // do assertions directly on the responseStream
    }
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Quickstart.cs#L88-L108' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_should_say_hello_world_with_raw_objects' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


