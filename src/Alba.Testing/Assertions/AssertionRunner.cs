using System;
using Alba.Stubs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Alba.Testing.Assertions
{
    public static class AssertionRunner
    {
        public static ScenarioAssertionException Run(IScenarioAssertion assertion,
            Action<HttpContext> configuration)
        {
            var ex = new ScenarioAssertionException();

            var context = StubHttpContext.Empty();


            configuration(context);
            
            var stream = context.Response.Body;
            stream.Position = 0;

            assertion.Assert(null, context, ex);

            return ex;
        }

        public static void SingleMessageShouldBe(this ScenarioAssertionException ex, string message)
        {
            ex.Messages.ShouldHaveTheSameElementsAs(message);
        }
    }
}