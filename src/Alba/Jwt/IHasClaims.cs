using System.Security.Claims;

namespace Alba.Jwt
{
    public interface IHasClaims
    {
        void AddClaim(Claim claim);
    }
}