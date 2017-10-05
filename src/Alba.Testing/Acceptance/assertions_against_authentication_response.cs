using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using WebApp;
using Xunit;

namespace Alba.Testing.Acceptance
{
    public class assertions_against_authentication_response
    {
        [Fact]
        public async Task challenges_ntlm_negotiate()
        {
            using (var system = SystemUnderTest.ForStartup<Startup>())
            {
                await system.Scenario(_ =>
                {
                    _.WithWindowsAuthentication();
                    _.Get.Url("/auth/windowschallenge");

                    _.StatusCodeShouldBe(HttpStatusCode.Unauthorized);
                    _.Header(HeaderNames.WWWAuthenticate).ShouldHaveValues("NTLM", "Negotiate");
                });
            }
        }

        [Fact]
        public async Task challenges_ntlm_negotiate_with_user()
        {
            using (var system = SystemUnderTest.ForStartup<Startup>())
            {
                await system.Scenario(_ =>
                {
                    var user = new ClaimsPrincipal(new ClaimsIdentity(Enumerable.Empty<Claim>(), "Windows"));
                    _.WithWindowsAuthentication(user);
                    _.Get.Url("/auth/windowschallenge");

                    _.StatusCodeShouldBe(HttpStatusCode.Forbidden);
                });
            }
        }
    }
}
