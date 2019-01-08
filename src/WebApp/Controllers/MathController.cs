using System;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{

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
    
    
    public class MathController : Controller
    {
        [HttpGet("/math/add/{one}/{two}")]
        public OperationResult Add(int one, int two)
        {
            return new OperationResult
            {
                Answer = one + two
            };
        }

        [HttpPut("/math")]
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
        
        [HttpPost("/math")]
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
    }
}