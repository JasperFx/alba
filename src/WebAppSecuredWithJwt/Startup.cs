using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace WebAppSecuredWithJwt
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "WebAppSecuredWithJwt", Version = "v1"});
            });
            
            services.AddMicrosoftIdentityWebApiAuthentication(Configuration, "AzureAdB2C", "AzureAuthentication");
            IdentityModelEventSource.LogCompleteSecurityArtifact = true;
            IdentityModelEventSource.ShowPII = true;
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // A real application would pull all this information from configuration
                    // of course, but I'm hardcoding it in testing
                    options.Audience = "jwtsample";
                    options.ClaimsIssuer = "myapp";
                    
                    // don't worry about this, our JwtSecurityStub is gonna switch it off in
                    // tests
                    options.Authority = "https://localhost:5001";
                    
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("some really big key that should work")),
                        NameClaimType = ClaimTypes.NameIdentifier
                    };
                }).AddJwtBearer("custom", options =>
                {
                    // A real application would pull all this information from configuration
                    // of course, but I'm hardcoding it in testing
                    options.Audience = "jwtsample";
                    options.ClaimsIssuer = "myapp";
                    
                    // don't worry about this, our JwtSecurityStub is gonna switch it off in
                    // tests
                    options.Authority = "https://localhost:5001";
                    
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("some really big key that should work")),
                        NameClaimType = ClaimTypes.NameIdentifier
                    };
                });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAppSecuredWithJwt v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}