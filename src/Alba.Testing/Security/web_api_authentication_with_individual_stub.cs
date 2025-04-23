using System.Net;
using Alba.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Shouldly;

namespace Alba.Testing.Security;

public class web_api_authentication_with_individual_stub
{
    [Fact]
    public async Task can_stub_individual_scheme()
    {
        #region sample_bootstrapping_with_stub_scheme_extension
        // Stub out an individual scheme
        var securityStub = new AuthenticationStub("custom")
            .With("foo", "bar")
            .With(JwtRegisteredClaimNames.Email, "guy@company.com")
            .WithName("jeremy");
        
        await using var host = await AlbaHost.For<WebAppSecuredWithJwt.Program>(securityStub);
        #endregion

        await host.Scenario(s =>
        {
            s.Get.Url("/identity2");
            s.StatusCodeShouldBeOk();
        });
        
        await host.Scenario(s =>
        {
            s.Get.Url("/identity");
            s.StatusCodeShouldBe(HttpStatusCode.Unauthorized);
        });
        
    }
    
    [Fact]
    public async Task can_stub_individual_scheme_jwt()
    {
        // This is a Alba extension that can "stub" out authentication
        var securityStub = new JwtSecurityStub(JwtBearerDefaults.AuthenticationScheme)
            .With("foo", "bar")
            .With(JwtRegisteredClaimNames.Email, "guy@company.com")
            .WithName("jeremy");

        // We're calling your real web service's configuration
        await using var host = await AlbaHost.For<WebAppSecuredWithJwt.Program>(securityStub);

        await host.Scenario(s =>
        {
            s.Get.Url("/identity");
            s.StatusCodeShouldBeOk();
        });
        
        await host.Scenario(s =>
        {
            s.Get.Url("/identity2");
            s.StatusCodeShouldBe(HttpStatusCode.Unauthorized);
        });
        
    }
    
    [Fact]
    public async Task can_stub_individual_schemes_microsoft_identity_web()
    {       
        var securityStub1 = new AuthenticationStub("custom")
            .With("foo", "bar")
            .With(JwtRegisteredClaimNames.Email, "guy@company.com")
            .WithName("jeremy");
        
        var securityStub2 = new JwtSecurityStub(JwtBearerDefaults.AuthenticationScheme)
            .With("foo", "bar")
            .With(JwtRegisteredClaimNames.Email, "guy@company.com")
            .WithName("jeremy");
        
        var securityStub3 = new JwtSecurityStub("AzureAuthentication")
            .With("iss", "bar")
            .With("tid", "tenantid")
            .With("roles", "")
            .With(JwtRegisteredClaimNames.Email, "guy@company.com")
            .WithName("jeremy");

        // We're calling your real web service's configuration
        await using var host = await AlbaHost.For<WebAppSecuredWithJwt.Program>(securityStub1, securityStub2, securityStub3);
        var postConfigures = host.Services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get("AzureAuthentication");
        
        postConfigures.ConfigurationManager.ShouldBeOfType<StaticConfigurationManager<OpenIdConnectConfiguration>>();
        
        await host.Scenario(s =>
        {
            s.Get.Url("/identity");
            s.StatusCodeShouldBeOk();
        });
        
        await host.Scenario(s =>
        {
            s.Get.Url("/identity2");
            s.StatusCodeShouldBeOk();
        });
        
        await host.Scenario(s =>
        {
            s.Get.Url("/identity3");
            s.StatusCodeShouldBeOk();
        });
        
    }
}