using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Newtonsoft.Json;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    public class JsonController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Json(new Person());
        }

        [HttpPost]
        public IActionResult Post()
        {
             var person = new JsonSerializer().Deserialize<Person>(new JsonTextReader(new StreamReader(HttpContext.Request.Body)));

             return Json(person);
        }

        [HttpPut]
        public IActionResult Put(Person person)
        {
            return Json(person);
        }
    }
    
    public class Person
    {
        public string FirstName = "Jeremy";
        public string LastName = "Miller";
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
            return contentType == "text/json" || contentType == "application/json";
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