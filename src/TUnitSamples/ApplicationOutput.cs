using Alba;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using TUnit.Logging.Microsoft;

namespace TUnitSamples;

public class ApplicationOutput
{
    [Test]
    public async Task console_output_written_while_handling_a_scenario_goes_to_the_test()
    {
        await using var host = await AlbaHost.For(WebApplication.CreateBuilder(), app =>
        {
            app.MapGet("/work", () =>
            {
                Console.WriteLine("Handled by the application");
                return "done";
            });
        });

        await host.Scenario(x => x.Get.Url("/work"));

        await Assert.That(TestContext.Current!.GetStandardOutput()).Contains("Handled by the application");
    }

    [Test]
    public async Task logs_written_while_handling_a_scenario_go_to_the_test()
    {
        var builder = WebApplication.CreateBuilder();

        #region sample_TUnit_correlated_logging
        // Writes the application's logs to the test that sent the request
        builder.Services.AddCorrelatedTUnitLogging();
        #endregion

        await using var host = await AlbaHost.For(builder, app =>
        {
            app.MapGet("/work", (ILogger<ApplicationOutput> logger) =>
            {
                logger.LogInformation("Logged by the application");
                return "done";
            });
        });

        await host.Scenario(x => x.Get.Url("/work"));

        await Assert.That(TestContext.Current!.GetStandardOutput()).Contains("Logged by the application");
    }
}
