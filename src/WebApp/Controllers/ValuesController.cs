using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly IEnumerable<IWidget> _lastWidget;
        public static IWidget[] LastWidget { get; set; }

        public ValuesController(IEnumerable<IWidget> lastWidget)
        {
            _lastWidget = lastWidget;
            
        }

        // GET api/values
        [HttpGet]
        public string Get()
        {
            HttpContext.Response.Headers.Append("content-type", "text/plain");

            return "value1, value2";
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public Task Post()
        {
            var reader = new StreamReader(HttpContext.Request.Body);
            var value = reader.ReadToEnd();
            return HttpContext.Response.WriteAsync("I ran a POST with value " + value);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
            LastWidget = _lastWidget.ToArray();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}