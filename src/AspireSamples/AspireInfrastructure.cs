using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AspireSamples;

#region sample_aspire_assembly_fixture_registration
// One AppHost, and one set of containers, for every test in the assembly
[assembly: AssemblyFixture<AspireInfrastructure>]
#endregion

namespace AspireSamples;

#region sample_aspire_infrastructure_fixture
public class AspireInfrastructure : IAsyncLifetime
{
    private DistributedApplication? _app;

    public string LedgerConnectionString { get; private set; } = null!;
    public string MessagingConnectionString { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        // The first run pulls the container images, so allow for a slow start
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(5));

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AspireSamples_AppHost>(timeout.Token);

        // Keep container logs, and the health check failures logged while
        // the containers start, out of the test output
        appHost.Services.AddLogging(logging => logging
            .SetMinimumLevel(LogLevel.Warning)
            .AddFilter("AspireSamples.AppHost.Resources", LogLevel.None)
            .AddFilter("Microsoft.Extensions.Diagnostics.HealthChecks", LogLevel.None));

        // Alba runs the API in process, so Aspire only starts its dependencies
        appHost.Resources.Remove(appHost.Resources.Single(x => x.Name == "api"));

        _app = await appHost.BuildAsync(timeout.Token);
        await _app.StartAsync(timeout.Token);

        // Wait on the database rather than the server, which is healthy before the database exists
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("ledger", timeout.Token);
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("messaging", timeout.Token);

        LedgerConnectionString = (await _app.GetConnectionStringAsync("ledger", timeout.Token))!;
        MessagingConnectionString = (await _app.GetConnectionStringAsync("messaging", timeout.Token))!;
    }

    public async ValueTask DisposeAsync()
    {
        // Stops the AppHost and removes its containers
        if (_app != null) await _app.DisposeAsync();
    }
}
#endregion
