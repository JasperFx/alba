namespace MinimalApiWithoutArgs;

/// <summary>
/// A minimal application that never forwards command line arguments into its
/// WebApplicationBuilder, so configuration can only reach it through the builder itself
/// </summary>
public class Program
{
    public static void Main()
    {
        var builder = WebApplication.CreateBuilder();

        // Read before Build so tests can prove overridden configuration reaches startup code
        var greeting = builder.Configuration["Alba:Greeting"] ?? "Hello from appsettings";

        var app = builder.Build();
        app.MapGet("/greeting", () => greeting);

        app.Run();
    }
}
