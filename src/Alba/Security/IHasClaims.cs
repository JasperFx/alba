using System.Security.Claims;

namespace Alba.Security;

/// <summary>
/// Implemented by types that model user permissions by claims
/// </summary>
public interface IHasClaims
{
    /// <summary>
    /// Add a baseline claim that will be added to the ClaimsPrincipal
    /// for any scenario executed by the current AlbaHost
    /// </summary>
    /// <param name="claim"></param>
    void AddClaim(Claim claim);
}