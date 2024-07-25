using Alba;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspireApp.Tests;

[Collection(nameof(WebAppCollection))]
public class IntegrationTest
{
    private readonly IAlbaHost _alba;

    public IntegrationTest(WebAppFixture fixture)
    {
        _alba = fixture.Alba;
    }
    
     [Fact]
     public async Task CanCreateWeatherForecast()
     {
         var forecast = new CreateWeatherForecastRequest(DateOnly.FromDateTime(DateTime.Now), 23, "etc");
         await _alba.Scenario(x =>
         {
             x.Post.Json(forecast)
                 .ToUrl("weatherforcast");
             x.StatusCodeShouldBeOk();
         });

         var forecasts = await _alba.GetAsJson<WeatherForecast[]>("weatherforcast");

         Assert.Single(forecasts!);
     }
}


[CollectionDefinition(nameof(WebAppCollection))]
public class WebAppCollection : ICollectionFixture<WebAppFixture>
{
}


public sealed class WebAppFixture : IAsyncLifetime
{
    public IAlbaHost Alba = null!;
    private IHost _aspireHost = null!;
    
    public async Task DisposeAsync()
    {
        await Alba.Services.GetRequiredService<IDocumentStore>().Advanced.Clean.DeleteAllDocumentsAsync();
        
        await Alba.DisposeAsync();
        if (_aspireHost is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
            _aspireHost.Dispose();
        }
    }

    public async Task InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AspireApp_AppHost>();
        appHost.Resources.Remove(appHost.Resources.Single(r => r.Name == "webfrontend"));
        appHost.Resources.Remove(appHost.Resources.Single(r => r.Name == "apiservice"));
        var postgres = (IResourceWithConnectionString)appHost.Resources.Single(x => x.Name == "postgres");
        _aspireHost = await appHost.BuildAsync();
        await _aspireHost.StartAsync();
        
        var connectionString = await postgres.GetConnectionStringAsync();
        Alba = await AlbaHost.For<Program>(x =>
        {
            x.ConfigureAppConfiguration((context, builder) =>
            {
                builder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { $"ConnectionStrings:{postgres.Name}", connectionString },
                });
            });
        });
    }
}




