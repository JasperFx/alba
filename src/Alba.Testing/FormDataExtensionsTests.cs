using System.Collections.Generic;
using Alba.Stubs;
using Baseline;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class FormDataExtensionsTests
    {

        [Fact]
        public void round_trip_writing_and_parsing()
        {
            var form1 = new Dictionary<string, string>
            {
                ["a"] = "what?",
                ["b"] = "now?",
                ["c"] = "really?"
            };

            var context = StubHttpContext.Empty();

            context.WriteFormData(form1);

            context.Request.Body.ReadAllText()
                .ShouldBe("a=what?&b=now?&c=really?");

        }


    }
}