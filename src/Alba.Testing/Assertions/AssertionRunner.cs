using System.Text;
using Microsoft.AspNetCore.Http;

namespace Alba.Testing.Assertions
{
    public static class AssertionRunner
    {
        // Test helper for writing to stub DefaultHttpContext responses, which
        // are backed by plain MemoryStreams
        public static void Write(this HttpResponse response, string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            response.Body.Write(bytes, 0, bytes.Length);
        }

        public static ScenarioAssertionException Run(IScenarioAssertion assertion,
            Action<HttpContext> configuration)
        {
            var ex = new ScenarioAssertionException();

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            context.Request.Body = new MemoryStream();


            configuration(context);
            
            var stream = context.Response.Body;
            stream.Position = 0;

            assertion.Assert(null, new AssertionContext(context, ex));

            return ex;
        }

        public static void SingleMessageShouldBe(this ScenarioAssertionException ex, string message)
        {
            ex.Messages.ShouldHaveTheSameElementsAs(message);
        }
    }
}