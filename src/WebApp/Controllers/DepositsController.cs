using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [Route("api/deposits")]
    [ApiController]
    public class DepositsController : ControllerBase
    {
        // [ApiController] answers invalid requests with a validation problem before this runs
        [HttpPost]
        public IActionResult Post(DepositRequest request) => Ok(request);
    }

    public class DepositRequest
    {
        [Range(0.01, 1_000_000)]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; }
    }
}
