using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

 

namespace Alba.Security;

public abstract class OpenConnectExtension : IAlbaExtension
{
    internal static readonly string OverrideKey = "alba_oidc_override";

    private HttpClient _client = null!;
    private DiscoveryDocumentResponse? _disco;
    private TokenResponse? _cached;
        

    void IDisposable.Dispose()
    {
        _client.Dispose();
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        _client.Dispose();
        return ValueTask.CompletedTask;
    }

    async Task IAlbaExtension.Start(IAlbaHost host)
    {
        AssertValid();
            
        // This seems to be necessary to "bake" in the JwtBearerOptions modifications
        var options = host.Services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        _client = options.BackchannelHttpHandler != null ? new HttpClient(options.BackchannelHttpHandler) : new HttpClient();

        var authorityUrl = options.Authority;
        _disco = await _client.GetDiscoveryDocumentAsync(authorityUrl);
        if (_disco.IsError)
        {
            throw _disco.Exception;
        }

        host.BeforeEachAsync(ConfigureJwt);

    }
        
    /// <summary>
    /// User-supplied value for the Open Id Connect ClientId. This is required.
    /// </summary>
    public string? ClientId { get; set; }
        
    /// <summary>
    /// User-supplied value for the Open Id Connect ClientSecret. This is required.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Validate that all the necessary information like ClientSecret and ClientId have been
    /// supplied to this extension
    /// </summary>
    public abstract void AssertValid();

    public Task<TokenResponse> FetchToken(object? tokenCustomization)
    {
        if (_disco == null)
            throw new InvalidOperationException(
                "This operation is not possible without an existing OIDC discovery document");
        return FetchToken(_client, _disco, tokenCustomization);
    }

    public abstract Task<TokenResponse> FetchToken(HttpClient client, DiscoveryDocumentResponse? disco,
        object? tokenCustomization);

    private async Task<TokenResponse> determineJwt(HttpContext context)
    {
        if (context.Items.TryGetValue(OverrideKey, out var scenarioOverride))
        {
            return await FetchToken(_client, _disco, scenarioOverride);
        }

        _cached ??= await FetchToken(_client, _disco, null);

        return _cached;
    }
        
    internal async Task ConfigureJwt(HttpContext context)
    {
        var token = await determineJwt(context);
        context.SetBearerToken(token.AccessToken);
    }

    IHostBuilder IAlbaExtension.Configure(IHostBuilder builder)
    {
        return builder;
    }
}