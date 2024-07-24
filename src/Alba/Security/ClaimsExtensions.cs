using System.Security.Claims;

namespace Alba.Security;

public static class ClaimsExtensions
{
    /// <summary>
    /// Append a baseline Claim to be placed on all requests
    /// </summary>
    /// <param name="claims"></param>
    /// <param name="claim"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T With<T>(this T claims, Claim claim) where T : IHasClaims
    {
        claims.AddClaim(claim);
        return claims;
    }
        
    /// <summary>
    /// Add a baseline Claim to be placed on all requests
    /// </summary>
    /// <param name="claims"></param>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T With<T>(this T claims, string type, string value) where T : IHasClaims
    {
        var claim = new Claim(type, value);
        return claims.With(claim);
    }

    /// <summary>
    /// Add a baseline "name" claim to each request
    /// </summary>
    /// <param name="claims"></param>
    /// <param name="name"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T WithName<T>(this T claims, string name) where T : IHasClaims
    {
        var claim = new Claim(ClaimTypes.Name, name);
        return claims.With(claim);
    }
}