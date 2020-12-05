using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class GatewayController : Controller
    {
        [HttpPost]
        public IActionResult Insert([FromForm]InputModel callInfo)
        {
            LastInput = callInfo;

            return Ok();
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