using System.Security.Claims;

namespace Alba.Security
{
    public interface IHasClaims
    {
        void AddClaim(Claim claim);
    }
}