using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Oakton;

namespace MinimalApiWithOakton
{
    public class Program
    {
        public static Task<int> Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();
            app.MapGet("/", () => "Hello World!");
            return app.RunOaktonCommands(args);
        }
    }
}

