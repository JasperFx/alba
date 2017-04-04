using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [Route("[controller]/[action]")]
    public class GatewayController : Controller
    {
        [HttpPost]
        public Task<IActionResult> Insert(InputModel callInfo)
        {
            LastInput = callInfo;
            
            IActionResult result = Ok();
            return Task.FromResult(result);
        }

        public static InputModel LastInput { get; set; }
    }

    public class InputModel
    {
        public string One { get; set; }
        public string Two { get; set; }
        public string Three { get; set; }

    }
}