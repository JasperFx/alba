using System.Security.Claims;
using Alba.Authentication;

namespace Alba
{
    public static class ScenarioExtensions
    {
        // SAMPLE: with-windows-authentication-extension
        public static Scenario WithWindowsAuthentication(this Scenario scenario, ClaimsPrincipal user = null)
        {
            scenario.Configure = context => context.AttachAuthenticationHandler(new StubWindowsAuthHandler(context), user);
            return scenario;
        }
        // ENDSAMPLE
    }
}
