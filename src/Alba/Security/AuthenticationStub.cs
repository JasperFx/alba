using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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

    void IAlbaExtension.Configure(IAlbaHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton(this);
            services.AddSingleton<IAuthenticationSchemeProvider, MockSchemeProvider>();
        });
    }

    internal ClaimsPrincipal BuildPrincipal(HttpContext context)
    {
        var claims = allClaims(context);
        var identity = new ClaimsIdentity(claims, TestSchemaName);
        var principal = new ClaimsPrincipal(identity);
        return principal;
    }

    private sealed class MockSchemeProvider : AuthenticationSchemeProvider
    {
        private static readonly Task<AuthenticationScheme?> TestScheme = Task.FromResult<AuthenticationScheme?>(
            new AuthenticationScheme(TestSchemaName, TestSchemaName, typeof(MockAuthenticationHandler)));

        private readonly string? _overrideSchemaTarget;

        public MockSchemeProvider(AuthenticationStub authSchemaStub, IOptions<AuthenticationOptions> options)
            : base(options)
        {
            _overrideSchemaTarget = authSchemaStub.OverrideSchemeTargetName;
        }

        public override Task<AuthenticationScheme?> GetSchemeAsync(string name)
        {
            if (_overrideSchemaTarget is null || name.Equals(_overrideSchemaTarget, StringComparison.OrdinalIgnoreCase))
            {
                return TestScheme;
            }

            return base.GetSchemeAsync(name);
        }

        private sealed class MockAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>, IAuthenticationSignOutHandler
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

            public Task SignOutAsync(AuthenticationProperties? properties)
            {
                return Task.CompletedTask;
            }
        }
    }
}
