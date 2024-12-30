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

namespace Alba.Security;

public sealed class AuthenticationStub : AuthenticationExtensionBase, IAlbaExtension
{
    private const string TestSchemaName = "Test";

    internal string? OverrideSchemeTargetName { get; }

    /// <summary>
    /// Creates a new authentication stub. Will override all implementations by default.
    /// </summary>
    /// <param name="overrideSchemeTargetName">Override a specific authentication schema.</param>
    public AuthenticationStub(string? overrideSchemeTargetName = null)
        => OverrideSchemeTargetName = overrideSchemeTargetName;

    void IDisposable.Dispose()
    {
        // nothing to dispose
    }

    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;

    Task IAlbaExtension.Start(IAlbaHost host) => Task.CompletedTask;

    IHostBuilder IAlbaExtension.Configure(IHostBuilder builder)
    {
        return builder.ConfigureServices(services =>
        {
            services.AddSingleton(this);
            services.AddAuthentication(OverrideSchemeTargetName ?? TestSchemaName);
            services.AddTransient<IAuthenticationSchemeProvider, MockSchemeProvider>();
            services.AddTransient<IAuthenticationHandlerProvider, StubAuthenticationHandlerProvider>();
        });
    }

    internal ClaimsPrincipal BuildPrincipal(HttpContext context)
    {
        var claims = allClaims(context);
        var identity = new ClaimsIdentity(claims, TestSchemaName);
        var principal = new ClaimsPrincipal(identity);
        return principal;
    }

    private sealed class StubAuthenticationHandlerProvider : IAuthenticationHandlerProvider, IAuthenticationHandler
    {
        private readonly AuthenticationStub _authSchemaStub;
        private HttpContext _context;

        public StubAuthenticationHandlerProvider(AuthenticationStub authSchemaStub)
        {
            _authSchemaStub = authSchemaStub;
        }

        public Task<IAuthenticationHandler?> GetHandlerAsync(HttpContext context, string authenticationScheme)
        {
            _context = context;
            return Task.FromResult<IAuthenticationHandler?>(this);
        }

        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            _context = context;
            return Task.CompletedTask;
        }

        public Task<AuthenticateResult> AuthenticateAsync()
        {
            var principal = _authSchemaStub.BuildPrincipal(_context);
            var ticket = new AuthenticationTicket(principal, TestSchemaName);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        public Task ChallengeAsync(AuthenticationProperties? properties)
        {
            return Task.CompletedTask;
        }

        public Task ForbidAsync(AuthenticationProperties? properties)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class MockSchemeProvider : AuthenticationSchemeProvider
    {
        private readonly string? _overrideSchemaTarget;

        public MockSchemeProvider(AuthenticationStub authSchemaStub, IOptions<AuthenticationOptions> options)
            : base(options)
        {
            _overrideSchemaTarget = authSchemaStub.OverrideSchemeTargetName;
        }

        public override Task<AuthenticationScheme?> GetSchemeAsync(string name)
        {
            if(_overrideSchemaTarget == null)
                return Task.FromResult(new AuthenticationScheme(
                    TestSchemaName,
                    TestSchemaName,
                    typeof(MockAuthenticationHandler)))!;
            if (name.Equals(_overrideSchemaTarget, StringComparison.OrdinalIgnoreCase))
            {
                var scheme = new AuthenticationScheme(
                    TestSchemaName,
                    TestSchemaName,
                    typeof(MockAuthenticationHandler));

                return Task.FromResult(scheme)!;
            }

            return base.GetSchemeAsync(name);
        }

        private sealed class MockAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
        {
            private readonly AuthenticationStub _authenticationSchemaStub;


            public MockAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, AuthenticationStub authenticationSchemaStub) : base(options, logger, encoder)
            {
                _authenticationSchemaStub = authenticationSchemaStub;
            }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                var principal = _authenticationSchemaStub.BuildPrincipal(Context);
                var ticket = new AuthenticationTicket(principal, TestSchemaName);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
        }
    }
}