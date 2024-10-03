using System.Threading.Tasks;
using Alba;
using NUnit.Framework;

namespace NUnitSamples;

#region sample_NUnit_Application

[SetUpFixture]
public class Application
{
    [OneTimeSetUp]
    public async Task Init()
    {
        Host = await AlbaHost.For<WebApp.Program>();
    }
        
    public static IAlbaHost Host { get; private set; }

    // Make sure that NUnit will shut down the AlbaHost when
    // all the projects are finished
    [OneTimeTearDown]
    public void Teardown()
    {
        Host.Dispose();
    }
}

#endregion

#region sample_NUnit_scenario_test
public class sample_integration_fixture
{
    [Test]
    public async Task happy_path()
    {
        await Application.Host.Scenario(_ =>
        {
            _.Get.Url("/fake/okay");
            _.StatusCodeShouldBeOk();
        });
    }
}
#endregion