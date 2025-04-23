# Integrating with TUnit

Like other testing frameworks, you'll want to reuse the `IAlbaHost` across tests and test fixtures because
`AlbaHost` is relatively expensive to create. To do that with TUnit, you should start by writing a bootstrapping class
 that inherits from `IAsyncInitializer` and `IAsyncDisposable`:

<!-- snippet: sample_TUnit_Application -->
<a id='snippet-sample_tunit_application'></a>
```cs
public sealed class AlbaBootstrap : IAsyncInitializer, IAsyncDisposable
{
    public IAlbaHost Host { get; private set; } = null!;

    public async Task InitializeAsync() 
    {
        Host = await AlbaHost.For<WebApp.Program>();
    }

    public async ValueTask DisposeAsync()
    {
        await Host.DisposeAsync();
    }
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/TUnitSamples/Program.cs#L6-L21' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_tunit_application' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Then inject the instance by adding `[ClassDataSource<AlbaBootstrap>(Shared = SharedType.PerTestSession)]` to your test class. We recommend creating a base class to allow easier access of the host and any other dependencies.

<!-- snippet: sample_TUnit_scenario_test -->
<a id='snippet-sample_tunit_scenario_test'></a>
```cs
public abstract class AlbaTestBase(AlbaBootstrap albaBootstrap)
{
    protected IAlbaHost Host => albaBootstrap.Host;
}

[ClassDataSource<AlbaBootstrap>(Shared = SharedType.PerTestSession)]
public class MyTestClass(AlbaBootstrap albaBootstrap) : AlbaTestBase(albaBootstrap)
{
    [Test]
    public async Task happy_path()
    {
        await Host.Scenario(_ =>
        {
            _.Get.Url("/fake/okay");
            _.StatusCodeShouldBeOk();
        });
    }
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/TUnitSamples/Program.cs#L23-L42' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_tunit_scenario_test' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
