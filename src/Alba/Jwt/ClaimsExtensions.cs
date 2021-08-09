using System.Security.Claims;

namespace Alba.Jwt
{
    public static class ClaimsExtensions
    {
        public static T With<T>(this T claims, Claim claim) where T : IHasClaims
        {
            claims.AddClaim(claim);
            return claims;
        }
        
        public static T With<T>(this T claims, string type, string value) where T : IHasClaims
        {
            var claim = new Claim(type, value);
            return claims.With(claim);
        }
    }
}