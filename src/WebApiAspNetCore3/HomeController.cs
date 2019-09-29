using Microsoft.AspNetCore.Mvc;

namespace WebApiAspNetCore3
{
    public class HomeController : Controller
    {
        [HttpGet("/")]
        public string Index()
        {
            return "Hello world.";
        }
    }
}