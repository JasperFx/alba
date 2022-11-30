using System.Collections.Generic;
using System.IO;
using System.Net;
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
            using var stream = new MemoryStream();
            context.Request.Body = stream;

            context.WriteFormData(form1);

            context.Request.Body.Position = 0;

            context.Request.Body.ReadAllText()
                .ShouldBe("a=what%3F&b=now%3F&c=really%3F");

        }


    }
}