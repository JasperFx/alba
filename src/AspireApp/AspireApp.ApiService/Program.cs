using Marten;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
// calling this directly gives me the wrong extension for whatever reason
AspirePostgreSqlNpgsqlExtensions.AddNpgsqlDataSource(builder,"apiservicedb");
builder.Services.AddMarten(x =>
{
    x.CreateDatabasesForTenants(c=> 
        c.ForTenant()
            .CheckAgainstPgDatabase()
        );
    
    x.UseSystemTextJsonForSerialization();
    
}).ApplyAllDatabaseChangesOnStartup()
    .UseNpgsqlDataSource();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapGet("/weatherforecast", async (IQuerySession session) =>
{
    return await session.Query<WeatherForecast>().ToListAsync();
});

app.MapPost("/weatherforcast", async (CreateWeatherForecastRequest request, IDocumentSession session) =>
{
    var forecast = new WeatherForecast(Guid.NewGuid(), request.Date, request.TemperatureC, request.Summary);

    session.Store(forecast);

    await session.SaveChangesAsync();
});

app.MapDefaultEndpoints();

app.Run();


public partial class Program {}
public record CreateWeatherForecastRequest(DateOnly Date, int TemperatureC, string? Summary);

public record WeatherForecast(Guid Id, DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}