---
title: Testing with Aspire and Docker Dependencies
description: Start real databases and message brokers from an Aspire AppHost with Aspire.Hosting.Testing and pass their connection strings to AlbaHost.
---

# Aspire Hosting

Alba runs your application in process, but the application usually still needs real infrastructure such as a database
or a message broker. If the system already has an [Aspire](https://aspire.dev) AppHost, the test suite can reuse it through
the `Aspire.Hosting.Testing` package. The AppHost starts those dependencies in Docker once for the whole test assembly, and
their connection strings flow into the `AlbaHost` configuration.

The samples on this page come from the `src/AspireSamples` project in the Alba repository. They use xUnit 4, Postgres and
RabbitMQ, and they need Docker to run.

## The AppHost

This is an ordinary AppHost for an API that depends on a Postgres database and a RabbitMQ broker:

<!-- snippet: sample_aspire_apphost -->
<a id='snippet-sample_aspire_apphost'></a>
```cs
var builder = DistributedApplication.CreateBuilder(args);

var ledger = builder.AddPostgres("postgres").AddDatabase("ledger");
var messaging = builder.AddRabbitMQ("messaging");

// WithReference hands the API ConnectionStrings:ledger and ConnectionStrings:messaging
builder.AddProject<Projects.AspireSamples_Api>("api")
    .WithReference(ledger).WaitFor(ledger)
    .WithReference(messaging).WaitFor(messaging);

builder.Build().Run();
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/AspireSamples.AppHost/Program.cs#L1-L13' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_aspire_apphost' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The tests reuse this AppHost as it is. `WithReference` gives the API `ConnectionStrings:ledger` and
`ConnectionStrings:messaging`. The Alba fixture below supplies the same configuration keys, so the application code does not
change between running under Aspire and running under Alba.

## Starting the AppHost once per test assembly

Add a package reference to `Aspire.Hosting.Testing` and a project reference to the AppHost in the test project. An xUnit
assembly fixture then starts the AppHost before the first test and stops it after the last one:

<!-- snippet: sample_aspire_infrastructure_fixture -->
<a id='snippet-sample_aspire_infrastructure_fixture'></a>
```cs
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
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/AspireSamples/AspireInfrastructure.cs#L15-L58' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_aspire_infrastructure_fixture' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

- `DistributedApplicationTestingBuilder.CreateAsync<T>()` runs the AppHost with defaults suited to testing. The
  `Projects.AspireSamples_AppHost` type is generated from the project reference.
- The API resource is removed before the AppHost starts, because Alba runs the API in process instead. Aspire only starts
  the containers.
- `WaitForResourceHealthyAsync` waits until a resource's health checks pass. Wait on the database resource rather than the
  Postgres server: the server reports healthy before Aspire has created the database.
- `GetConnectionStringAsync` returns the connection string of the running container, including its generated password and
  mapped port.
- Disposing the `DistributedApplication` stops the AppHost and removes its containers.

Register the fixture once for the assembly:

<!-- snippet: sample_aspire_assembly_fixture_registration -->
<a id='snippet-sample_aspire_assembly_fixture_registration'></a>
```cs
// One AppHost, and one set of containers, for every test in the assembly
[assembly: AssemblyFixture<AspireInfrastructure>]
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/AspireSamples/AspireInfrastructure.cs#L8-L11' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_aspire_assembly_fixture_registration' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Sharing the AppHost with Alba fixtures

xUnit injects assembly fixtures into collection fixture constructors, so the fixture that owns the `AlbaHost` receives the
running infrastructure and passes the connection strings to the application with `WithConfiguration`:

<!-- snippet: sample_aspire_alba_fixture -->
<a id='snippet-sample_aspire_alba_fixture'></a>
```cs
public class ApiFixture(AspireInfrastructure infrastructure) : IAsyncLifetime
{
    public IAlbaHost Host { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        // The same configuration keys that WithReference() sets when Aspire runs the API
        Host = await AlbaHost.For<AspireSamples.Api.Program>()
            .WithConfiguration("ConnectionStrings:ledger", infrastructure.LedgerConnectionString)
            .WithConfiguration("ConnectionStrings:messaging", infrastructure.MessagingConnectionString);
    }

    public async ValueTask DisposeAsync()
    {
        await Host.DisposeAsync();
    }
}

[CollectionDefinition(nameof(ApiCollection))]
public class ApiCollection : ICollectionFixture<ApiFixture>;
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/AspireSamples/ApiFixture.cs#L5-L26' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_aspire_alba_fixture' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Any number of Alba fixtures, collections and test classes can take `AspireInfrastructure` the same way, and they all share
the one AppHost and its containers. xUnit disposes collection fixtures before assembly fixtures, so every `AlbaHost` shuts
down before the containers it uses are removed.

## Writing scenarios

Scenarios run against the real database and broker. Test classes can also take `AspireInfrastructure` directly to check side
effects at the source, as the second test does with the RabbitMQ queue:

<!-- snippet: sample_aspire_scenarios -->
<a id='snippet-sample_aspire_scenarios'></a>
```cs
[Collection(nameof(ApiCollection))]
public class deposit_specs(ApiFixture api)
{
    [Fact]
    public async Task deposits_add_up_to_the_balance()
    {
        // The database is shared by every test, so each test uses its own account
        var accountId = Guid.NewGuid();

        await api.Host.Scenario(x =>
        {
            x.Post.Json(new Deposit(accountId, 100m)).ToUrl("/deposits");
            x.StatusCodeShouldBe(201);
        });

        await api.Host.Scenario(x =>
        {
            x.Post.Json(new Deposit(accountId, 50m)).ToUrl("/deposits");
            x.StatusCodeShouldBe(201);
        });

        var balance = await api.Host.GetAsJson<Balance>($"/accounts/{accountId}/balance");
        balance!.Amount.ShouldBe(150m);
    }
}

[Collection(nameof(ApiCollection))]
public class deposit_messaging_specs(ApiFixture api, AspireInfrastructure infrastructure)
{
    [Fact]
    public async Task every_deposit_is_published_to_the_queue()
    {
        var deposit = new Deposit(Guid.NewGuid(), 25m);

        await api.Host.Scenario(x =>
        {
            x.Post.Json(deposit).ToUrl("/deposits");
            x.StatusCodeShouldBe(201);
        });

        // Read the broker directly with the connection string the API was given
        var factory = new ConnectionFactory { Uri = new Uri(infrastructure.MessagingConnectionString) };
        await using var connection = await factory.CreateConnectionAsync(TestContext.Current.CancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: TestContext.Current.CancellationToken);

        (await ReceiveAsync(channel, deposit.AccountId)).ShouldBe(deposit);
    }

    // Other tests publish to the same queue, so skip past their deposits
    private static async Task<Deposit?> ReceiveAsync(IChannel channel, Guid accountId)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (!timeout.IsCancellationRequested)
        {
            var message = await channel.BasicGetAsync(DepositPublisher.QueueName, autoAck: true, timeout.Token);
            if (message == null)
            {
                await Task.Delay(100, timeout.Token);
                continue;
            }

            var deposit = JsonSerializer.Deserialize<Deposit>(message.Body.Span);
            if (deposit?.AccountId == accountId) return deposit;
        }

        return null;
    }
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/AspireSamples/DepositSpecs.cs#L9-L78' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_aspire_scenarios' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Every test shares the same containers, so give each test its own data, such as a fresh account id here, instead of relying
on an empty database.

## Tips

- The first run pulls the container images, which can take several minutes. The fixture above allows five minutes for
  startup. Later runs start in seconds.
- On .NET 10, `AlbaHost.For<T>` passes `WithConfiguration` values to the application as host configuration, so the
  application must call `WebApplication.CreateBuilder(args)`. See
  [Configuring the Host Fluently](gettingstarted.md#configuring-the-host-fluently).
- Build agents need Docker. GitHub's hosted Linux runners have it.
- With NUnit or TUnit, start the AppHost in a [`[SetUpFixture]`](nunit.md) or in a [`ClassDataSource`](tunit.md) shared
  per test session, then build the `AlbaHost` from its connection strings the same way.
