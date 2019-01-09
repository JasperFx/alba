using System.Threading.Tasks;
using Alba.Testing.Samples;
using Shouldly;
using WebApp.Controllers;
using Xunit;

namespace Alba.Testing
{
    public class using_json_helpers
    {
        // SAMPLE: get-json
        [Fact]
        public async Task get_happy_path()
        {
            // SystemUnderTest is from Alba
            // The "Startup" type would be the Startup class from your
            // web application. 
            using (var system = SystemUnderTest.ForStartup<WebApp.Startup>())
            {
                // Issue a request, and check the results
                var result = await system.GetAsJson<OperationResult>("/math/add/3/4");
                
                result.Answer.ShouldBe(7);
            }
        }
        // ENDSAMPLE

        // SAMPLE: post-json-get-json
        [Fact]
        public async Task post_and_expect_response()
        {
            using (var system = SystemUnderTest.ForStartup<WebApp.Startup>())
            {
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
        }
        // ENDSAMPLE
        
        [Fact]
        public async Task put_and_expect_response()
        {
            using (var system = SystemUnderTest.ForStartup<WebApp.Startup>())
            {
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
}