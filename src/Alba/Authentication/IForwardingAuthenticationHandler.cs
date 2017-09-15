using Microsoft.AspNetCore.Http.Features.Authentication;

namespace Alba.Authentication
{
    public interface IForwardingAuthenticationHandler : IAuthenticationHandler
    {
        IAuthenticationHandler PriorHandler { get; set; }
    }
}