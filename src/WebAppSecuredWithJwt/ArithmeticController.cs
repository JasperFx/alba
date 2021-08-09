using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi
{
    public class Result
    {
        public int Sum { get; set; }
        public int Product { get; set; }
    }

    public class Numbers
    {
        public int[] Values { get; set; }
    }
    
    public class ArithmeticController : ControllerBase
    {
        [Authorize]
        [HttpPost("/math")]
        public Result DoMath([FromBody] Numbers input)
        {
            var product = 1;
            foreach (var value in input.Values)
            {
                product *= value;
            }

            return new Result
            {
                Sum = input.Values.Sum(),
                Product = product
            };
        }
    }
}