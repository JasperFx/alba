using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class ServiceController : Controller
    {
        private readonly ISimpleService _simpleService;

        public ServiceController(ISimpleService simpleService)
        {
            _simpleService = simpleService;
        }

        [HttpGet("test-service")]
        public string Get()
        {
            return _simpleService.Test();
        }
    }

    public class SimpleService : ISimpleService
    {
        public string Test()
        {
            return "test";
        }
    }

    public interface ISimpleService
    {
        string Test();
    }
}
