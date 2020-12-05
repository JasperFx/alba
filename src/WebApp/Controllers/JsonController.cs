using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
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

    public class TextInputFormatter : IInputFormatter
    {
        public bool CanRead(InputFormatterContext context)
        {
            return context.HttpContext.Request.ContentType == "text/plain";
        }

        public async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            var text = await context.HttpContext.Request.Body.ReadAllTextAsync();
            return InputFormatterResult.Success(text);
        }
    }

    public class JsonInputFormatter : IInputFormatter
    {
        public JsonInputFormatter()
        {
            Console.WriteLine("I was called");
        }

        public bool CanRead(InputFormatterContext context)
        {
            var contentType = context.HttpContext.Request.ContentType;

            var supported = new[] { "text/json", "application/json"};

            return supported.Any(x => contentType?.StartsWith(x) ?? false);
        }

        public Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            var serializer = new JsonSerializer();

            var model = serializer.Deserialize(new JsonTextReader(new StreamReader(context.HttpContext.Request.Body)), context.ModelType);

            var result = InputFormatterResult.Success(model);

            return Task.FromResult(result);
        }
    }
}