using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Alba.Security
{
    public abstract class SecurityStub : IHasClaims
    {
        private readonly IList<Claim> _baselineClaims = new List<Claim>();


        void IHasClaims.AddClaim(Claim claim)
        {
            _baselineClaims.Add(claim);
        }

        protected IEnumerable<Claim> allClaims(Claim[] claims)
        {
            foreach (var claim1 in stubTypeSpecificClaims()) yield return claim1;

            foreach (var claim in _baselineClaims) yield return claim;

            foreach (var claim in claims) yield return claim;
        }

        protected virtual IEnumerable<Claim> stubTypeSpecificClaims()
        {
            yield break;
        }

        protected Claim[] extractScenarioSpecificClaims(HttpContext context)
        {
            if (context.Items.TryGetValue("alba_claims", out var raw))
            {
                if (raw is Claim[] cs)
                {
                    return cs;
                }
            }

            return Array.Empty<Claim>();
        }

        protected IEnumerable<Claim>? allClaims(HttpContext context)
        {
            var claims = extractScenarioSpecificClaims(context);

            return allClaims(claims);
        }
    }
}