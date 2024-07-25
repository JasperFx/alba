using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FakeController : ControllerBase
    {
        [HttpGet("{status}")]
        public IResult Fake(string status)
        {
            switch (status)
            {
                case "bad":
                    throw new DivideByZeroException("Boom!");

                case "invalid":
                    return Results.Problem("It's all wrong", title: "This stinks!");

                default:

                    return Results.Ok("it's all good");
            }
        }
    }
}