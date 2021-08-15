using System;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    #region sample_MathController
    public enum OperationType
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }
    
    public class OperationRequest
    {
        public OperationType Type { get; set; }
        public int One { get; set; }
        public int Two { get; set; }
    }

    public class OperationResult
    {
        public int Answer { get; set; }
        public string Method { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class MathController : Controller
    {
        [HttpGet("add/{one}/{two}")]
        public OperationResult Add(int one, int two)
        {
            return new OperationResult
            {
                Answer = one + two
            };
        }

        [HttpPut]
        public OperationResult Put([FromBody]OperationRequest request)
        {
            switch (request.Type)
            {
                case OperationType.Add:
                    return new OperationResult{Answer = request.One + request.Two, Method = "PUT"};
                
                case OperationType.Multiply:
                    return new OperationResult{Answer = request.One * request.Two, Method = "PUT"};
                
                case OperationType.Subtract:
                    return new OperationResult{Answer = request.One - request.Two, Method = "PUT"};
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(request.Type));
            }
        }
        
        [HttpPost]
        public OperationResult Post([FromBody]OperationRequest request)
        {
            switch (request.Type)
            {
                case OperationType.Add:
                    return new OperationResult{Answer = request.One + request.Two, Method = "POST"};
                    
                case OperationType.Multiply:
                    return new OperationResult{Answer = request.One * request.Two, Method = "POST"};
                
                case OperationType.Subtract:
                    return new OperationResult{Answer = request.One - request.Two, Method = "POST"};
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(request.Type));
            }
        }
        
        #endregion
    }
}
