using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Alba.Security;

/// <summary>
/// Use this extension to generate and apply JWT tokens to scenario requests using
/// a set of baseline claims
/// </summary>
public class JwtSecurityStub : AuthenticationExtensionBase, IAlbaExtension
{
    private JwtBearerOptions? _options;

    private readonly string? _overrideSchemaTargetName;
    public JwtSecurityStub(string? overrideSchemaTargetName = null)
        => _overrideSchemaTargetName = overrideSchemaTargetName;
    
    void IDisposable.Dispose()
    {
        // Nothing
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    Task IAlbaExtension.Start(IAlbaHost host)
    {
        // This seems to be necessary to "bake" in the JwtBearerOptions modifications
        var options = host.Services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(_overrideSchemaTargetName ?? JwtBearerDefaults.AuthenticationScheme);


        host.BeforeEach(ConfigureJwt);
        return Task.CompletedTask;
    }

    internal void ConfigureJwt(HttpContext context)
    {
        var (additiveClaims, removedClaims) = extractScenarioSpecificClaims(context);
        var jwt = BuildJwtString(additiveClaims, removedClaims);

        context.SetBearerToken(jwt);
    }

    IHostBuilder IAlbaExtension.Configure(IHostBuilder builder)
    {
        return builder.ConfigureServices(services =>
        {
            if (_overrideSchemaTargetName != null)
            {
                services.PostConfigure<JwtBearerOptions>(_overrideSchemaTargetName, PostConfigure);
            }
            else
            {
                services.PostConfigureAll<JwtBearerOptions>(PostConfigure);
            }
           
        });
    }

    internal SecurityTokenDescriptor BuildToken(Claim[]? additionalClaims = null, string[]? removedClaims = null)
    {
        if (_options == null)
            throw new InvalidOperationException("Unable to determine the JwtBearerOptions for this AlbaHost");

        var algorithm = _options.TokenValidationParameters.ValidAlgorithms?.FirstOrDefault()
                        ?? SecurityAlgorithms.HmacSha256;

        var audience = _options.TokenValidationParameters.ValidAudiences?.FirstOrDefault() 
                       ?? _options.TokenValidationParameters.ValidAudience 
                       ?? _options.Audience;
            
        var credentials = new SigningCredentials(
            _options.TokenValidationParameters.IssuerSigningKey, 
            algorithm);

        return new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(processedClaims(additionalClaims, removedClaims)),
            Issuer = _options.ClaimsIssuer,
            Audience = audience,
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = credentials
        };
    }
        
    internal string BuildJwtString(Claim[] additionalClaims, string[]? removedClaims = null)
    {
        var tokenHandler = new JsonWebTokenHandler();
        var tokenDescriptor = BuildToken(additionalClaims, removedClaims);
        return tokenHandler.CreateToken(tokenDescriptor);
    }

    protected override IEnumerable<Claim> stubTypeSpecificClaims()
    {
        yield return new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString());
        if (_options?.ClaimsIssuer != null)
        {
            yield return new Claim(JwtRegisteredClaimNames.Iss, _options.ClaimsIssuer);
        }
    }

    void PostConfigure(JwtBearerOptions options)
    {
        // This will deactivate the callout to the OIDC server
        options.ConfigurationManager =
            new StaticConfigurationManager<OpenIdConnectConfiguration>(new OpenIdConnectConfiguration
            {
                    
            });
            
        var validationParameters = options.TokenValidationParameters.Clone();
        validationParameters.IssuerSigningKey ??= new SymmetricSecurityKey("some really big key that should work"u8.ToArray());
        validationParameters.ValidateIssuer = false;
        validationParameters.IssuerValidator = (issuer, token, parameters) => issuer;
        options.TokenValidationParameters = validationParameters;

        options.Authority = null;
        options.MetadataAddress = null!;

        _options = options;
    }

    internal JwtBearerOptions? Options
    {
        get => _options;
        set
        {
            _options = value ?? throw new ArgumentNullException(nameof(value));
            _options.TokenValidationParameters.IssuerSigningKey ??= new SymmetricSecurityKey(Encoding.UTF8.GetBytes("some really big key that should work"));
        }
    }
}