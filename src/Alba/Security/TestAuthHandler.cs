using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Alba.Security
{
    internal class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly HttpContext _context;
        private readonly AuthenticationStub _parent;

        public TestAuthHandler(IHttpContextAccessor accessor, AuthenticationStub parent, IOptionsMonitor<AuthenticationSchemeOptions> options, 
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _context = accessor.HttpContext ?? throw new InvalidOperationException("HttpContext is missing");
            
            _parent = parent;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var principal = _parent.BuildPrincipal(_context);
            var ticket = new AuthenticationTicket(principal, "Test");

            var result = AuthenticateResult.Success(ticket);

            return Task.FromResult(result);
        }
    }
}