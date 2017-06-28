using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class QueryStringContoller : Controller
    {
        [HttpGet("query-string")]
        public string QueryString([FromQuery] string test)
        {
            return test;
        }
    }
}