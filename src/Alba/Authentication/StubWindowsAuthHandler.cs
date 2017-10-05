using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features.Authentication;

namespace Alba.Authentication
{
    // SAMPLE: stub-windows-auth-handler
    public class StubWindowsAuthHandler : AuthenticationHandler
    {
        public StubWindowsAuthHandler(HttpContext httpContext)
            : base(httpContext)
        {
        }

        public override void GetDescriptions(DescribeSchemesContext context)
        {
            var schemes = new Dictionary<string, object>
            {
                {"DisplayName", "NTLM"},
                {"AuthenticationScheme", "NTLM"}
            };

            context.Accept(schemes);

            base.GetDescriptions(context);
        }

        protected override bool ShouldHandleScheme(string scheme)
        {
            return string.Equals("NTLM", scheme, StringComparison.OrdinalIgnoreCase)
                   || string.Equals("Negotiate", scheme, StringComparison.OrdinalIgnoreCase);
        }
    }
    // ENDSAMPLE
}