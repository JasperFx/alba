using System.Linq;
using JasperFx.Core;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [ApiController]
    public class QueryStringContoller : Controller
    {
        [HttpGet("querystring")]
        public string QueryString([FromQuery] string test)
        {
            return test;
        }
        
        [HttpGet("querystringarray")]
        public string QueryStringList([FromQuery] string[] tests)
        {
            return string.Join(',', tests);
        }

        [HttpPost("sendform")]
        public string SendForm([FromForm] string test)
        {
            return test;
        }

        [HttpPost("sendbody")]
        public string SendBody([FromBody] string text)
        {
            return text;
        }

        [HttpGet("querystring2")]
        public string ReadQueryString()
        {
            var queryString = HttpContext.Request.Query;

            if (queryString.Count == 0)
            {
                return "No query string parameters";
            }

            return queryString.Select(x => $"{x.Key}={x.Value}").Join(";");
        }

        
    }
}