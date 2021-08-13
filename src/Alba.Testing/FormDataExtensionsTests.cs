using System.Collections.Generic;
using Baseline;
using Microsoft.AspNetCore.Http;
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

            var context = new DefaultHttpContext();

            context.WriteFormData(form1);

            context.Request.Body.ReadAllText()
                .ShouldBe("a=what?&b=now?&c=really?");

        }


    }
}