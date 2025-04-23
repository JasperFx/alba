# Getting Started

## What is Alba?

Alba is a class library that you use in combination with unit testing tools like [xUnit.Net](https://xunit.github.io) or [NUnit](https://docs.nunit.org/) to author integration tests
against ASP.NET Core HTTP endpoints. Alba *scenarios* actually exercise the full ASP.NET Core application by running HTTP requests through your ASP.NET system **in memory** using the 
built in [ASP.NET Core TestServer](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0).

You can certainly write integration tests by hand using the lower level `TestServer` and `HttpClient`, but you'll write much less code with Alba. Moreover, Alba *scenarios* were meant to be declarative to maximize the readability of the integration tests, making those tests much more valuable as living technical documentation.

::: tip
As of 8.0+, Alba only supports .NET 8.0 or greater. You can still use older versions of Alba to test previous versions of ASP.NET Core.
:::

## Alba Setup

To get started with Alba, add a Nuget reference to the Alba library to your testing project that also references the ASP.NET Core
project that you're going to be testing. 

In the following sections I'll show you how to bootstrap
your ASP.NET Core system with Alba and start authoring specifications with the `AlbaHost` type.

## Initializing AlbaHost

Alba is compatible with both traditional-style `Startup.cs` projects as well as the new `WebApplicationBuilder` minimal approach. The following instructions work with both models.

As an example, consider this very small ASP.NET Core application utilizing the new [Minimal API](https://docs.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-6.0&tabs=visual-studio) approach:

<!-- snippet: sample_minimal_web_api -->
<a id='snippet-sample_minimal_web_api'></a>
```cs
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello World!");
app.MapGet("/blowup", context => throw new Exception("Boo!"));
app.MapPost("/json", (MyEntity entity) => entity);

app.Run();

public record MyEntity(Guid Id, string MyValue);
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/WebApiNet6/Program.cs#L1-L23' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_minimal_web_api' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

**As this is a Minimal API project, you will need to allow your test project access to the internal types of your application under test**. You
can do that by either using the [InternalsVisibleToAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.internalsvisibletoattribute?view=net-6.0) in your main
application project, or use this within the project file of your application where "ProjectName.Tests" would be your testing project name:

```xml
  <ItemGroup>
    <InternalsVisibleTo Include="ProjectName.Tests" />
  </ItemGroup>
```

We can now scaffold the `AlbaHost` using `AlbaHost.For<T>`, where `T` is your applications entry point:

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
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Acceptance/web_application_factory_usage.cs#L42-L51' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_bootstrapping_with_web_application_factory' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

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
    // Alba will automatically manage the lifetime of the underlying host
    await using var host = await AlbaHost.For<global::Program>();
    
    // This runs an HTTP request and makes an assertion
    // about the expected content of the response
    await host.Scenario(_ =>
    {
        _.Get.Url("/");
        _.ContentShouldBe("Hello World!");
        _.StatusCodeShouldBeOk();
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Quickstart.cs#L13-L29' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_should_say_hello_world' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


The `Action<Scenario>` argument will completely configure the ASP.NET `HttpContext` for the request and apply
any of the declarative response assertions. The actual HTTP request happens inside of the `Scenario()` method. 

The `Scenario` response contains the raw `HttpContext` and several helper methods to help you parse or read information from the response body:

<!-- snippet: sample_should_return_entity_assert_response -->
<a id='snippet-sample_should_return_entity_assert_response'></a>
```cs
[Fact]
public async Task should_return_entity_assert_response()
{
    await using var host = await AlbaHost.For<global::Program>();

    var guid = Guid.NewGuid();
    var res = await host.Scenario(_ =>
    {
        _.Post.Json(new MyEntity(guid, "SomeValue")).ToUrl("/json");
        _.StatusCodeShouldBeOk();
    });

    var json = await res.ReadAsJsonAsync<MyEntity>();
    Assert.Equal(guid, json.Id);
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Quickstart.cs#L31-L47' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_should_return_entity_assert_response' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

If the existing *Scenario* assertions aren't enough to verify your test case, you can work directly against the raw response:

<!-- snippet: sample_should_say_hello_world_with_raw_objects -->
<a id='snippet-sample_should_say_hello_world_with_raw_objects'></a>
```cs
[Fact]
public async Task should_say_hello_world_with_raw_objects()
{
    await using var host = await AlbaHost.For<global::Program>();
    var response = await host.Scenario(_ =>
    {
        _.Get.Url("/");
        _.StatusCodeShouldBeOk();
    });

    // you can go straight at the HttpContext & do assertions directly on the responseStream
    Stream responseStream = response.Context.Response.Body;

}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Quickstart.cs#L51-L66' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_should_say_hello_world_with_raw_objects' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Do note that Alba quietly "rewinds" the `HttpContext.Response.Body` stream so that you can more readily read and work with the contents.

## Customizing the System for Testing

You can configure your application with mocked services or test-specific configuration like so:

<!-- snippet: sample_configuration_overrides -->
<a id='snippet-sample_configuration_overrides'></a>
```cs
var stubbedWebService = new StubbedWebService();

await using var host = await AlbaHost.For<global::Program>(x =>
{
    // override the environment if you need to
    x.UseEnvironment("Testing");
    // override service registrations or internal options if you need to
    x.ConfigureServices(s =>
    {
        s.AddSingleton<IExternalWebService>(stubbedWebService);
        s.PostConfigure<MvcNewtonsoftJsonOptions>(o =>
            o.SerializerSettings.TypeNameHandling = TypeNameHandling.All);
    });
});

host.BeforeEach(httpContext =>
    {
        // do some data setup or clean up before every single test
    })
    .AfterEach(httpContext =>
    {
        // do any kind of cleanup after each scenario completes
    });
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Quickstart.cs#L93-L117' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_configuration_overrides' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Alba does not do anything to set the hosting environment, but you can do that yourself via the `IWebHostBuilder`

