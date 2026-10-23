using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [ApiController]
    public class NamedRouteController : ControllerBase
    {
        [HttpGet("/named/accounts/{accountId:int}", Name = "GetMvcAccount")]
        public string Get(int accountId) => $"account {accountId}";
    }
}
