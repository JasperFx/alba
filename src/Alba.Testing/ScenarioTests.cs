using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class ScenarioTests : ScenarioContext
    {
        


        [Fact]
        public Task invoke_a_simple_string_endpoint()
        {
            host.Handlers["/memory/hello"] = c =>
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



        public Task examples()
        {
            return host.Scenario(_ =>
            {
                // Specify a GET request to the Url that runs an endpoint method:
                _.Get.Action<InMemoryEndpoint>(e => e.get_memory_hello());

                // Or specify a POST to the Url that would handle an input message:
                _.Post

                    // This call serializes the input object to Json using the 
                    // application's configured JSON serializer and setting
                    // the contents on the Request body
                    .Json(new HeaderInput { Key = "Foo", Value1 = "Bar" });

                // Or specify a GET by an input object to get the route parameters
                _.Get.Input(new InMemoryInput { Color = "Red" });
            });


            /*
            host.Scenario(_ =>
            {
                // set up a request here

                // Read the response body as text
                var bodyText = _.Response.Body.ReadAsText();

                // Read the response body by deserializing Json
                // into a .net type with the application's
                // configured Json serializer
                var output = _.Response.Body.ReadAsJson<MyResponse>();

                // If you absolutely have to work with Xml...
                var xml = _.Response.Body.ReadAsXml();
            });
            */
        }

        [Fact]
        public Task using_scenario_with_string_text_and_relative_url()
        {
            host.Handlers["/memory/hello"] = c =>
            {
                c.Response.Write("hello from the in memory host");
                return Task.CompletedTask;
            };

            return host.Scenario(x =>
            {
                x.Get.Url("/memory/hello");
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