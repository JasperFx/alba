using System;
using System.Collections.Generic;
using System.IO;
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

        [Fact]
        public void round_trip_writing_file()
        {
            var form1 = new Dictionary<string, string> { ["a"] = "what?" };
            var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            
            var context = StubHttpContext.Empty();
            
            context.WriteMultipartFormData(form1, Path.Combine(projectDirectory, "IFormFile.txt"), "document");

            context.Request.Form.Files[0].ContentDisposition
                .ShouldBe("form-data; filename=IFormFile.txt; name=document");

            context.Request.Form.Files[0].ContentType
                .ShouldBe("text/plain");
        }
    }
}