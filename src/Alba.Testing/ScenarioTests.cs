using Alba.Internal;
using Shouldly;

namespace Alba.Testing
{
    public class ScenarioTests : ScenarioContext
    {
        


        [Fact]
        public Task invoke_a_simple_string_endpoint()
        {
            router.Handlers["/memory/hello"] = c =>
            {
                c.Response.Write("hello from the in memory host");
                return Task.CompletedTask;
            };

            return host.Scenario(_ =>
            {
                _.Get.Url("/memory/hello");
                _.ContentShouldBe("hello from the in memory host");
            });
        }

        [Fact]
        public Task using_scenario_with_string_text_and_relative_url()
        {
            router.Handlers["/memory/hello"] = c =>
            {
                c.Response.Write("hello from the in memory host");
                return Task.CompletedTask;
            };

           return host.Scenario(x =>
            {
                x.Get.Url("/memory/hello");
                x.StatusCodeShouldBeOk();
                
                x.ContentShouldBe("hello from the in memory host");
            });
        }

        [Fact]
        public Task using_scenario_with_byte_array_and_content_type()
        {
            var bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z/C/HgAGgwJ/lK3Q6wAAAABJRU5ErkJggg==");
            const string contentType = "image/png";
            router.Handlers["/memory/bytes"] = c =>
            {
                c.Request.Body.ReadAllBytes().ShouldBe(bytes);
                c.Request.ContentType.ShouldBe(contentType);
                c.Response.Write("hello from the in memory host");
                return Task.CompletedTask;
            };
            
            return host.Scenario(x =>
            {
                x.Post.ByteArray(bytes).ToUrl("/memory/bytes").ContentType(contentType);
                x.StatusCodeShouldBeOk();
                
                x.ContentShouldBe("hello from the in memory host");
            });
        }
    }

    public class InMemoryEndpoint
    {
        
        private readonly IDictionary<string, object> _writer;

        public InMemoryEndpoint(IDictionary<string, object> writer)
        {
            _writer = writer;
        }
        

        public string post_header_values(HeaderInput input)
        {
            throw new NotImplementedException();

//            if (input.Value1.IsNotEmpty())
//            {
//                _writer.AppendHeader(input.Key, input.Value1);
//            }
//
//            if (input.Value2.IsNotEmpty())
//            {
//                _writer.AppendHeader(input.Key, input.Value2);
//            }
//
//            return "it's all good";
        }

        public string get_memory_hello()
        {
            return "hello from the in memory host";
        }

        public string get_memory_color_Color(InMemoryInput input)
        {
            return "The color is " + input.Color;
        }

        public string get_memory_marker_text(MarkerInput input)
        {
            return "just the marker";
        }
    }

    public class HeaderInput
    {
        public string Key { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
    }

    public class InMemoryInput
    {
        public string Color { get; set; }
    }

    public class MarkerInput
    {
    }
}