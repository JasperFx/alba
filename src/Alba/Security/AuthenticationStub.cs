using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alba.Security
{
    /// <summary>
    /// Stubs out security in all Alba scenarios to always authenticate
    /// a user on each request with the configured claims
    /// </summary>
    public class AuthenticationStub : AuthenticationExtensionBase, IAlbaExtension
    {
        void IDisposable.Dispose()
        {
        }

        ValueTask IAsyncDisposable.DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        Task IAlbaExtension.Start(IAlbaHost host)
        {
            return Task.CompletedTask;
        }

        IHostBuilder IAlbaExtension.Configure(IHostBuilder builder)
        {
            return builder.ConfigureServices(services =>
            {
                services.AddHttpContextAccessor();
                services.AddSingleton(this);
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "Test", _ => {});
            });
        }

        internal ClaimsPrincipal BuildPrincipal(HttpContext context)
        {
            var claims = allClaims(context);
            var identity = new ClaimsIdentity(claims, "Test");

            var principal = new ClaimsPrincipal(identity);

            return principal;
        }
    }
}