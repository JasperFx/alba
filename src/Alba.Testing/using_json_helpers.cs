using Shouldly;
using WebApp.Controllers;

namespace Alba.Testing
{
    public class using_json_helpers
    {
        #region sample_get_json
        [Fact]
        public async Task get_happy_path()
        {
            await using var system = await AlbaHost.For<WebApp.Program>();
            
            // Issue a request, and check the results
            var result = await system.GetAsJson<OperationResult>("/math/add/3/4");
                
            result.Answer.ShouldBe(7);
        }
        #endregion

        #region sample_post_json_get_json
        [Fact]
        public async Task post_and_expect_response()
        {
            await using var system = await AlbaHost.For<WebApp.Program>();
            var request = new OperationRequest
            {
                Type = OperationType.Multiply,
                One = 3,
                Two = 4
            };

            var result = await system.PostJson(request, "/math")
                .Receive<OperationResult>();
                
            result.Answer.ShouldBe(12);
            result.Method.ShouldBe("POST");
        }
        #endregion
        
        [Fact]
        public async Task put_and_expect_response()
        {
            await using var system = await AlbaHost.For<WebApp.Program>();
            var request = new OperationRequest
            {
                Type = OperationType.Subtract,
                One = 3,
                Two = 4
            };

            var result = await system.PutJson(request, "/math")
                .Receive<OperationResult>();
                
            result.Answer.ShouldBe(-1);
            result.Method.ShouldBe("PUT");
        }
    }
}
