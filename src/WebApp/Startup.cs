using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using WebApp.Controllers;
using JsonInputFormatter = WebApp.Controllers.JsonInputFormatter;

namespace WebApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IWidget, GreenWidget>();

            var mvcBuilder = services.AddMvc(config =>
            {
                config.RespectBrowserAcceptHeader = true;
                config.InputFormatters.Clear();
                config.InputFormatters.Add(new TextInputFormatter());
                config.InputFormatters.Add(new JsonInputFormatter());

#if NETCOREAPP3_0
                config.EnableEndpointRouting = false;
#endif
            });
            
#if NETCOREAPP3_0
            mvcBuilder.AddNewtonsoftJson();
#endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                FileProvider = new PhysicalFileProvider(env.ContentRootPath)
            });
            
            app.UseProblemDetails(x =>
            {
                
                // This is the default behavior; only include exception details in a development environment.
                x.IncludeExceptionDetails = ctx => env.IsDevelopment();
            });

            app.UseMvc();
        }
    }
}