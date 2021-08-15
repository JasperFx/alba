using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AuthController : Controller
    {
        #region sample_windows_challenge_endpoint
        public IActionResult WindowsChallenge()
        {
            return new ChallengeResult(new List<string> {"NTLM", "Negotiate"});
        }
        #endregion

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
