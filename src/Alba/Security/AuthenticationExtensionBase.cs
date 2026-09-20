using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Alba.Security;

/// <summary>
/// Base class for extensions that apply authentication mechanics to AlbaHosts
/// </summary>
public abstract class AuthenticationExtensionBase : IHasClaims
{
    private readonly List<Claim> _baselineClaims = new();


    void IHasClaims.AddClaim(Claim claim)
    {
        _baselineClaims.Add(claim);
    }

    private IEnumerable<Claim> defaultClaims()
    {
        foreach (var claim1 in stubTypeSpecificClaims()) yield return claim1;

        foreach (var claim in _baselineClaims) yield return claim;
    }

    protected IEnumerable<Claim> processedClaims(Claim[]? additiveClaims, string[]? removedClaims)
    {
        var claims = defaultClaims().ToList();
        if(removedClaims is not null)
            claims.RemoveAll(c => removedClaims.Contains(c.Type));
        if(additiveClaims is not null)
            claims.AddRange(additiveClaims);
        return claims;
    }

    protected virtual IEnumerable<Claim> stubTypeSpecificClaims()
    {
        return Enumerable.Empty<Claim>();
    }

    protected (Claim[] additiveClaims, string[] removedClaims) extractScenarioSpecificClaims(HttpContext context)
    {
        var additiveClaims = context.Items.TryGetValue(Scenario.ClaimsItemKey, out var raw) && raw is Claim[] added
            ? added
            : [];

        var removedClaims = context.Items.TryGetValue(Scenario.RemovedClaimsItemKey, out var rawRemoved)
                            && rawRemoved is string[] removed
            ? removed
            : [];

        return (additiveClaims, removedClaims);
    }

    protected IEnumerable<Claim> allClaims(HttpContext context)
    {
        var (additiveClaims, removedClaims) = extractScenarioSpecificClaims(context);

        return processedClaims(additiveClaims, removedClaims);
    }
}
