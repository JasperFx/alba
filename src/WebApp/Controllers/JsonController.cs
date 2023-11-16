using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JasperFx.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JsonController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Json(new Person());
        }

        [HttpPost]
        public IActionResult Post([FromBody]Person person)
        {
             return Json(person);
        }

        [HttpPut]
        public IActionResult Put([FromBody]Person person)
        {
            return Json(person);
        }
    }
    
    public class Person
    {
        public string FirstName = "Jeremy";
        public string LastName = "Miller";
    }

    public class TextInputFormatter : InputFormatter
    {
        public TextInputFormatter()
        {
            SupportedMediaTypes.Add("text/plain");
        }
        public override bool CanRead(InputFormatterContext context)
        {
            return context.HttpContext.Request.ContentType == "text/plain";
        }
        
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var text = await context.HttpContext.Request.Body.ReadAllTextAsync();
            return await InputFormatterResult.SuccessAsync(text);
        }
    }
}