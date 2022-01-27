# Alba Setup

To get started with Alba, just add a Nuget reference to the Alba library to your testing project that should also be referencing the ASP.Net Core
project that you're going to be testing. When using Alba, you actually bootstrap your web application in memory using either the "older"
[HostBuilder model](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.hostbuilder?view=dotnet-plat-ext-6.0) or the newer [WebApplicationFactory](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.testing.webapplicationfactory-1?view=aspnetcore-6.0) model , then pass the 
root `IHost` of the running application to Alba's `AlbaHost`, which will be the entry point to using Alba in all of your integration tests.

`AlbaHost` is shown in the class diagram below:

![AlbaHost Class Diagram](./../public/ClassDiagram.drawio.png)

`AlbaHost` implements the ASP.Net Core [IHost](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.ihost?view=dotnet-plat-ext-6.0) interface
for convenience and familiarity. Internally it is tracking the actual `IHost` for your application running in memory as well
as an instance of the ASP.Net Core [TestServer](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-6.0) that will actually be used to execute HTTP requests against the application in memory.

In the following sections I'll show you how to bootstrap
your ASP.Net Core system with Alba and start authoring specifications with the `AlbaHost` type.


## Initializing AlbaHost with IHostBuilder


To bootstrap a **ASP.NET Core 5** application, create a `AlbaHost` using the definition of your `IHostBuilder` as shown below:

<!-- snippet: sample_Quickstart3 -->
<a id='snippet-sample_quickstart3'></a>
```cs
[Fact]
public async Task build_host_from_Program()    
{
    // Bootstrap your application just as your real application does
    var hostBuilder = Program.CreateHostBuilder(Array.Empty<string>());

    await using var host = new AlbaHost(hostBuilder);

    // Just as a sample, I'll run a scenario against
    // a "hello, world" application's root url
    await host.Scenario(s =>
    {
        s.Get.Url("/");
        s.ContentShouldBe("Hello, World!");
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Quickstart3.cs#L15-L32' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_quickstart3' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

::: tip
There are both synchronous and asynchronous methods to bootstrap an `AlbaHost`. Depending on your test harness, I recommend using 
the asynchronous version whenever applicable.
:::

Or alternatively, you can use one of the Alba extension methods off of `IHostBuilder` to start an `AlbaHost` object in a fluent interface
style:

<!-- snippet: sample_shorthand_bootstrapping -->
<a id='snippet-sample_shorthand_bootstrapping'></a>
```cs
[Fact]
public async Task fluent_interface_bootstrapping()    
{
    await using var host = await Program
        .CreateHostBuilder(Array.Empty<string>())
        .StartAlbaAsync();

    // Just as a sample, I'll run a scenario against
    // a "hello, world" application's root url
    await host.Scenario(s =>
    {
        s.Get.Url("/");
        s.ContentShouldBe("Hello, World!");
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Quickstart3.cs#L35-L51' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_shorthand_bootstrapping' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The `AlbaHost` is an extension of the standard .Net Core [IHost](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.ihost?view=dotnet-plat-ext-5.0) interface with a few additions for testing support.
While you can always access the underlying `TestServer` through the `IAlbaHost.Server` property, you're mostly going to be using the `Scenario()` method to write Alba "Scenario" tests.

::: tip
To make the samples in this page easier to follow, I'm bootstrapping the `IAlbaHost` within each test. In real usage, bootstrapping your
application is expensive and you will probably want to reuse the `IAlbaHost` between tests. See the integrations with NUnit and xUnit.Net for examples.
:::

## Initializing AlbaHost with WebApplicationFactory

::: tip
This functionality was added in Alba 6.0 and is only supported in .Net 6+.
:::

For **ASP.NET Core 6**, Microsoft introduced a new mechanism for configuring and bootstrapping web applications using [WebApplicationBuilder](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.webapplicationbuilder?view=aspnetcore-6.0).

As an example, consider this very small ASP.Net Core application utilizing the new [Minimal API](https://docs.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-6.0&tabs=visual-studio) approach:

<!-- snippet: sample_minimal_web_api -->
<a id='snippet-sample_minimal_web_api'></a>
```cs
using System;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello World!");
app.MapGet("/blowup", context => throw new Exception("Boo!"));

app.Run();
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/WebApiNet6/Program.cs#L1-L22' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_minimal_web_api' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Using this project configuration mechanism, Alba is still usable, but this time we need to utilize ASP.Net Core's [WebApplicationFactory](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.testing.webapplicationfactory-1?view=aspnetcore-6.0)
tooling. Alba tries to make the usage of this a little easier with this syntax:

<!-- snippet: sample_bootstrapping_with_web_application_factory -->
<a id='snippet-sample_bootstrapping_with_web_application_factory'></a>
```cs
await using var host = await AlbaHost.For<global::Program>(x =>
{
    x.ConfigureServices((context, services) =>
    {
        services.AddSingleton<IService, ServiceA>();
    });
});
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Acceptance/web_application_factory_usage.cs#L46-L55' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_bootstrapping_with_web_application_factory' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

You'll need to add the following to your `.csproj` for `Program` to be discoverable by the test project:

```xml
  <ItemGroup>
    <InternalsVisibleTo Include="ProjectName.Tests" />
  </ItemGroup>
```

The `AlbaHost.For<T>(Action<WebApplicationFactory<T>> configuration)` method uses `WebApplicationFactory` and all its magic static
member trickery to intercept and run the implied `Program.Main()` method from the sample application above while also allowing you to customize 
the application configuration at testing time. The "T" in this case is only a marker type so that `WebApplicationFactory` can choose the correct 
entry assembly for the web application that is being tested by Alba.

See [this blog post from Andrew Lock on the WebApplicationFactory mechanics](https://andrewlock.net/exploring-dotnet-6-part-6-supporting-integration-tests-with-webapplicationfactory-in-dotnet-6/)
for more information.

::: tip
`AlbaHost` is an expensive object to create, so you'll generally want to reuse it across tests. See the relevant guide for [xUnit](xunit.md) or [NUnit](nunit.md)
:::


## Running a Scenario

Once you have a `AlbaHost` object, you're ready to execute *Scenario's* through your system inside of tests.
Below is a scenario for the "hello, world" application:

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


The single `Action<Scenario>` argument should completely configure the ASP.Net `HttpContext` for the request and apply
any of the declarative response assertions. The actual HTTP request happens inside of the `Scenario()` method. 
The response contains the raw `HttpContext` and several helper methods to help you parse or read information from the response body:

<!-- snippet: sample_IScenarioResult -->
<a id='snippet-sample_iscenarioresult'></a>
```cs
public interface IScenarioResult
{
    /// <summary>
    ///     Helpers to interrogate or read the HttpResponse.Body
    ///     of the request
    /// </summary>
    [Obsolete("Use the methods directly on IScenarioResult instead")]
    HttpResponseBody ResponseBody { get; }

    /// <summary>
    ///     The raw HttpContext used during the scenario
    /// </summary>
    HttpContext Context { get; }

    /// <summary>
    /// Read the contents of the HttpResponse.Body as text
    /// </summary>
    /// <returns></returns>
    string ReadAsText();

    /// <summary>
    /// Read the contents of the HttpResponse.Body into an XmlDocument object
    /// </summary>
    /// <returns></returns>
    XmlDocument? ReadAsXml();

    /// <summary>
    /// Deserialize the contents of the HttpResponse.Body into an object
    /// of type T using the built in XmlSerializer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T? ReadAsXml<T>() where T : class;

    /// <summary>
    /// Deserialize the contents of the HttpResponse.Body into an object
    /// of type T using the configured Json serializer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T? ReadAsJson<T>();

    T? Read<T>(string contentType);
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba/IScenarioResult.cs#L8-L53' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_iscenarioresult' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

::: tip
Alba v5 makes the old `HttpResponseBody` obsolete. It's still there, but the same functionality is readily available on `IScenarioResult`.
:::

If the existing *Scenario* assertions aren't enough to verify your test case, you can work directly against the raw response:

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

Do note that Alba quietly "rewinds" the `HttpContext.Response.Body` stream so that you can more readily read and work with the contents.

## Customizing the System for Testing

If you also want to run real HTTP requests through your system in a test harness, you have more opportunities to configure the underlying [IWebHostBuilder](https://docs.microsoft.com/en-us/aspnet/core/api/microsoft.aspnetcore.hosting.iwebhostbuilder) like so:

<!-- snippet: sample_configuration_overrides -->
<a id='snippet-sample_configuration_overrides'></a>
```cs
var stubbedWebService = new StubbedWebService();

var builder = Host.CreateDefaultBuilder()
    .ConfigureWebHostDefaults(c => c.UseStartup<Startup>())

    // override the environment if you need to
    .UseEnvironment("Testing")

    // override service registrations or internal options if you need to
    .ConfigureServices(s =>
    {
        s.AddSingleton<IExternalWebService>(stubbedWebService);
        s.PostConfigure<MvcNewtonsoftJsonOptions>(o =>
            o.SerializerSettings.TypeNameHandling = TypeNameHandling.All);
    });

// Create the SystemUnderTest
var system = new AlbaHost(builder)
    .BeforeEach(httpContext =>
    {
        // do some data setup or clean up before every single test
    })
    .AfterEach(httpContext =>
    {
        // do any kind of cleanup after each scenario completes
    });
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Quickstart.cs#L173-L200' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_configuration_overrides' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

A couple notes:

* Alba does not do anything to set the hosting environment, but you can do that yourself against `IWebHostBuilder`
* If you build a `AlbaHost` with `AlbaHost.ForStartup<T>()`, it will try to guess at the content root path by the name of assembly
  that holds the `Startup` class, but you may need to override that yourself. 

My shop is also using Alba within [Storyteller](http://storyteller.github.io) specifications where we use a mix of headless
Alba Scenario's and full HTTP requests for testing.

