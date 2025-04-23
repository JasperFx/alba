using Alba;
using TUnit.Core.Interfaces;

namespace TUnitSamples;

#region sample_TUnit_Application
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
#endregion

#region sample_TUnit_scenario_test
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
#endregion