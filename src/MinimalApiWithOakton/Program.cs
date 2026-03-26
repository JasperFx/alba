using JasperFx;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Http.Json;

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
            app.MapGet("/args", () => Results.Ok(args));
            app.MapPost("/go", (PostedMessage input) => new OutputMessage(input.Id));

            return app.RunJasperFxCommands(args);
        }
    }
    
    public record PostedMessage(Guid Id);
    public record OutputMessage(Guid Id);
}

