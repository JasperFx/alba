using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.Net.Http.Headers;

namespace Alba.Authentication
{
    public class AuthenticationHandler : IForwardingAuthenticationHandler
    {
        public AuthenticationHandler(HttpContext httpContext)
        {
            HttpContext = httpContext;
        }

        public HttpContext HttpContext { get; }
        public IAuthenticationHandler PriorHandler { get; set; }

        public virtual void GetDescriptions(DescribeSchemesContext context)
        {
            PriorHandler?.GetDescriptions(context);
        }

        public virtual Task AuthenticateAsync(AuthenticateContext context)
        {
            context.NotAuthenticated();

            PriorHandler?.AuthenticateAsync(context);

            return Task.CompletedTask;
        }

        public virtual Task ChallengeAsync(ChallengeContext context)
        {
            var handled = false;

            if (ShouldHandleScheme(context.AuthenticationScheme))
            {
                switch (context.Behavior)
                {
                    case ChallengeBehavior.Automatic:
                        // If there is a principal already, invoke the forbidden code path
                        if (GetUser() == null)
                        {
                            goto case ChallengeBehavior.Unauthorized;
                        }
                        else
                        {
                            goto case ChallengeBehavior.Forbidden;
                        }
                    case ChallengeBehavior.Unauthorized:
                        HttpContext.Response.StatusCode = 401;
                        On401(context);
                        break;
                    case ChallengeBehavior.Forbidden:
                        HttpContext.Response.StatusCode = 403;
                        On403(context);
                        handled = true;
                        break;
                }

                context.Accept();
            }

            if (!handled)
            {
                PriorHandler?.ChallengeAsync(context);
            }

            return Task.CompletedTask;
        }

        public virtual Task SignInAsync(SignInContext context)
        {
            PriorHandler?.SignInAsync(context);

            return Task.CompletedTask;
        }

        public virtual Task SignOutAsync(SignOutContext context)
        {
            PriorHandler?.SignOutAsync(context);

            return Task.CompletedTask;
        }

        protected virtual bool ShouldHandleScheme(string scheme)
        {
            return false;
        }

        protected virtual ClaimsPrincipal GetUser()
        {
            // Don't get it from httpContext.User, that always returns a non-null anonymous user by default.
            return HttpContext.Features.Get<IHttpAuthenticationFeature>()?.User;
        }

        protected virtual void On401(ChallengeContext context)
        {
            HttpContext.Response.Headers.Append(HeaderNames.WWWAuthenticate, context.AuthenticationScheme);
        }

        protected virtual void On403(ChallengeContext context)
        {
        }
    }
}
