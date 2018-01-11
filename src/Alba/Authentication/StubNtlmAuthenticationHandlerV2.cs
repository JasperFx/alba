#if NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.WebEncoders.Testing;
using Microsoft.Net.Http.Headers;

namespace Alba
{
    public class StubNtlmAuthenticationHandlerV2 : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ClaimsPrincipal user;

        public StubNtlmAuthenticationHandlerV2(ClaimsPrincipal user) 
            : base(GetEmptyOptions(), new NullLoggerFactory(), new UrlTestEncoder(), new SystemClock()) {
            this.user = user;
        }

        private static IOptionsMonitor<AuthenticationSchemeOptions> GetEmptyOptions() {
            return new OptionsMonitor<AuthenticationSchemeOptions>(
                new OptionsFactory<AuthenticationSchemeOptions>(
                    Enumerable.Empty<IConfigureOptions<AuthenticationSchemeOptions>>(),
                    Enumerable.Empty<IPostConfigureOptions<AuthenticationSchemeOptions>>()
                ),
                Enumerable.Empty<IOptionsChangeTokenSource<AuthenticationSchemeOptions>>(),
                new OptionsCache<AuthenticationSchemeOptions>()
            );
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
            if (user == null) {
                return Task.FromResult(AuthenticateResult.Fail("Nope"));
            }

            return Task.FromResult(
                AuthenticateResult.Success(new AuthenticationTicket(user, "NTLM"))
            );
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties) {
            Response.Headers[HeaderNames.WWWAuthenticate] = new StringValues(new[] {"NTLM", "Negotiate"});
            Response.StatusCode = user == null ? 401 : 403;

            return Task.CompletedTask;
        }
    }
}
#endif
