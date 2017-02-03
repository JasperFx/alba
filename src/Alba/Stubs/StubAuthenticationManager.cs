using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Authentication.Internal;
using Microsoft.AspNetCore.Http.Features.Authentication;

namespace Alba
{
    public class StubAuthenticationManager : AuthenticationManager
    {
        private readonly DefaultAuthenticationManager _default;

        public StubAuthenticationManager(HttpContext context)
        {
            HttpContext = context;
            _default = new DefaultAuthenticationManager(context);
        }

        public override IEnumerable<AuthenticationDescription> GetAuthenticationSchemes()
        {
            return _default.GetAuthenticationSchemes();
        }

        public override Task<AuthenticateInfo> GetAuthenticateInfoAsync(string authenticationScheme)
        {
            return _default.GetAuthenticateInfoAsync(authenticationScheme);
        }

        public override Task AuthenticateAsync(AuthenticateContext context)
        {
            return _default.AuthenticateAsync(context);
        }

        public override Task ChallengeAsync(string authenticationScheme, AuthenticationProperties properties, ChallengeBehavior behavior)
        {
            return _default.ChallengeAsync(authenticationScheme, properties, behavior);
        }

        public override Task SignInAsync(string authenticationScheme, ClaimsPrincipal principal, AuthenticationProperties properties)
        {
            return _default.SignInAsync(authenticationScheme, principal, properties);
        }

        public override Task SignOutAsync(string authenticationScheme, AuthenticationProperties properties)
        {
            return _default.SignOutAsync(authenticationScheme, properties);
        }

        public override HttpContext HttpContext { get; }
    }
}