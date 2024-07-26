using System.Net;
using System.Threading.Tasks;
using Alba.Security;
using Microsoft.IdentityModel.JsonWebTokens;
using Xunit;

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
        var securityStub = new JwtSecurityStub("custom")
            .With("foo", "bar")
            .With(JwtRegisteredClaimNames.Email, "guy@company.com")
            .WithName("jeremy");

        // We're calling your real web service's configuration
        await using var host = await AlbaHost.For<WebAppSecuredWithJwt.Program>(securityStub);

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
}