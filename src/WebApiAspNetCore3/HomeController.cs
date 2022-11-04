using Microsoft.AspNetCore.Mvc;

namespace WebApiStartupHostingModel
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