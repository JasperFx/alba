using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [Route("[controller]/[action]")]
    public class AuthController : Controller
    {
        public IActionResult WindowsChallenge()
        {
            return new ChallengeResult(new List<string> {"NTLM", "Negotiate"});
        }

        public IActionResult Redirect()
        {
            return RedirectToAction("Get", "Values");
        }

        public IActionResult RedirectPermanent()
        {
            return RedirectToActionPermanent("Get", "Values");
        }
    }
}
