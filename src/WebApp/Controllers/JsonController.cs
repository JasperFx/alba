using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Post(Person person)
        {
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
}