using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Alba.Testing.Samples
{
    public class Authentication
    {
        // SAMPLE: asserting-windows-auth
        public Task asserting_authentication(ISystemUnderTest system)
        {
            return system.Scenario(_ =>
            {
                _.WithWindowsAuthentication();
                _.Get.Url("/");

                _.StatusCodeShouldBe(HttpStatusCode.Unauthorized);
                _.Header("www-authenticate").ShouldHaveValues("NTLM", "Negotiate");
            });
        }
        // ENDSAMPLE

        // SAMPLE: asserting-windows-auth-with-user
        public Task asserting_authentication_with_user(ISystemUnderTest system)
        {
            return system.Scenario(_ =>
            {
                var user = new ClaimsPrincipal(new ClaimsIdentity(Enumerable.Empty<Claim>(), "Windows"));
                _.WithWindowsAuthentication(user);
                _.Get.Url("/");

                _.StatusCodeShouldBe(HttpStatusCode.Forbidden);
            });
        }
        // ENDSAMPLE
    }
}
