using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Http.Json;
using Oakton;

namespace MinimalApiWithOakton
{
    public class Program
    {
        public static Task<int> Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseLamar();
            
            // Configure JSON options.
            builder.Services.Configure<JsonOptions>(options =>
            {
                options.SerializerOptions.IncludeFields = true;
            });

            var app = builder.Build();
            app.MapGet("/", () => "Hello World!");

            app.MapPost("/go", (PostedMessage input) => new OutputMessage(input.Id));

            return app.RunOaktonCommands(args);
        }
    }
    
    public record PostedMessage(Guid Id);
    public record OutputMessage(Guid Id);
}

