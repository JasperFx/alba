using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Alba.Security
{
    /// <summary>
    /// Stubs out security in all Alba scenarios to always authenticate
    /// a user on each request with the configured claims
    /// </summary>
    public class AuthenticationStub : SecurityStub, IAlbaExtension
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
                        "Test", o => {});
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
    
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly HttpContext _context;
        private readonly AuthenticationStub _parent;

        public TestAuthHandler(IHttpContextAccessor accessor, AuthenticationStub parent, IOptionsMonitor<AuthenticationSchemeOptions> options, 
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _context = accessor.HttpContext ?? throw new InvalidOperationException("HttpContext is missing");
            
            _parent = parent;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var principal = _parent.BuildPrincipal(_context);
            var ticket = new AuthenticationTicket(principal, "Test");

            var result = AuthenticateResult.Success(ticket);

            return Task.FromResult(result);
        }
    }
}