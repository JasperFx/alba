using System;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FakeController : ControllerBase
    {
        [HttpGet("{status}")]
        public string Fake(string status)
        {
            switch (status)
            {
                case "bad":
                    throw new DivideByZeroException("Boom!");

                case "invalid":
                    throw new ProblemDetailsException(new ProblemDetails
                    {
                        Status = 400,
                        Detail = "It's all wrong",
                        Title = "This stinks!"
                    });

                default:

                    return "it's all good";
            }
        }
    }
}