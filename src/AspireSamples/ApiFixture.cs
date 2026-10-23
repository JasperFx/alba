using Alba;

namespace AspireSamples;

#region sample_aspire_alba_fixture
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
#endregion
