# Integrating with xUnit

If you are writing only a few Alba specifications in your testing project and your application spins up very quickly, you can just happily write tests like this:

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

Do note that your `[Fact]` method needs to be declared as `async Task` to ensure that xUnit finishes the specification before disposing the system or
you'll get *unusual* behavior. Also note that you really need to dispose the `AlbaHost` to shut down your application and dispose any internal services that might be holding on to computer resources.

## Class Fixtures

If your application startup time becomes a performance problem, and especially in larger test suites, you probably want to share the `AlbaHost` object between tests. xUnit helpfully provides the [class fixture feature](https://xunit.net/docs/shared-context) for just this use case. 

Build out your `AlbaHost` in a class like this:

<!-- snippet: sample_xUnit_Fixture_net6 -->
<a id='snippet-sample_xunit_fixture_net6'></a>
```cs
public class WebAppFixture : IAsyncLifetime
{
    public IAlbaHost AlbaHost = null!;

    public async ValueTask InitializeAsync()
    {
        AlbaHost = await Alba.AlbaHost.For<WebApp.Program>(builder =>
        {
            // Configure all the things
        });
    }

    public async ValueTask DisposeAsync()
    {
        await AlbaHost.DisposeAsync();
    }
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/ContractTestWithAlba.cs#L6-L25' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_xunit_fixture_net6' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Then in your actual xUnit fixture classes, implement the `IClassFixture<T>` class like this:

<!-- snippet: sample_using_xUnit_Fixture -->
<a id='snippet-sample_using_xunit_fixture'></a>
```cs
public class ContractTestWithAlba : IClassFixture<WebAppFixture>
{
    public ContractTestWithAlba(WebAppFixture app)
    {
        _host = app.AlbaHost;
    }

    private readonly IAlbaHost _host;
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/ContractTestWithAlba.cs#L28-L37' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_xunit_fixture' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Collection Fixtures

In the previous section, the `WebAppFixture` instance will only be shared between all the tests in the one `ContractTestWithAlba` class.
To reuse the `IAlbaHost` across multiple test fixture classes, you'll need to use xUnit.Net's [Collection Fixture](https://xunit.net/docs/shared-context) concept.

Still using `WebAppFixture`, we'll now need to have a marker collection class like this:

<!-- snippet: sample_ScenarioCollection -->
<a id='snippet-sample_scenariocollection'></a>
```cs
[CollectionDefinition("scenarios")]
public class ScenarioCollection : ICollectionFixture<WebAppFixture>
{
    
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/ContractTestWithAlba.cs#L74-L82' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_scenariocollection' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

As a convenience, I like to have a base class for all test fixture classes that will be using scenarios like this:

<!-- snippet: sample_ScenarioContext -->
<a id='snippet-sample_scenariocontext'></a>
```cs
[Collection("scenarios")]
public abstract class ScenarioContext
{
    protected ScenarioContext(WebAppFixture fixture)
    {
        Host = fixture.AlbaHost;
    }

    public IAlbaHost Host { get; }
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/ContractTestWithAlba.cs#L84-L97' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_scenariocontext' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

And then inherit from that `ScenarioContext` base class in actual test fixture classes:

<!-- snippet: sample_integration_fixture -->
<a id='snippet-sample_integration_fixture'></a>
```cs
public class sample_integration_fixture : ScenarioContext
{
    public sample_integration_fixture(WebAppFixture fixture) : base(fixture)
    {
    }
    
    [Fact]
    public Task happy_path()
    {
        return Host.Scenario(_ =>
        {
            _.Get.Url("/fake/okay");
            _.StatusCodeShouldBeOk();
        });
    }
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/ContractTestWithAlba.cs#L99-L118' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_integration_fixture' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
