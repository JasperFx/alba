using System.Runtime.CompilerServices;

namespace Alba.Testing.Samples;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize() =>
        VerifierSettings.InitializePlugins();
}

public class SnapshotTesting
{
    [Fact]
    public async Task SnapshotTest()
    {
        #region sample_snapshot_testing
        await using var host = await AlbaHost.For<global::Program>();

        var scenario = await host.Scenario(s =>
        {
            s.Post.Json(new MyEntity(Guid.NewGuid(), "SomeValue")).ToUrl("/json");
        });

        var body = scenario.ReadAsJson<MyEntity>();

        await Verify(new {
            scenario.Context,
            ResponseBody = body,
        });
        #endregion
    }
}