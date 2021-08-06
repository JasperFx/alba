using System;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;

namespace Alba.Testing.Samples
{
    public class JsonAndXml
    {
        // SAMPLE: sending-json
        public Task send_json(IScenarioRunner system)
        {
            return system.Scenario(_ =>
            {
                // This serializes the Input object to json,
                // writes it to the HttpRequest.Body, and sets
                // the accepts & content-type header values to
                // application/json
                _.Post
                    .Json(new Input {Name = "Max", Age = 13})
                    .ToUrl("/person");

                // OR, if url lookup is enabled, this is an equivalent:
                _.Post.Json(new Input {Name = "Max", Age = 13});
            });
        }
        // ENDSAMPLE

        // SAMPLE: sending-xml
        public Task send_xml(IScenarioRunner system)
        {
            return system.Scenario(_ =>
            {
                // This serializes the Input object to xml,
                // writes it to the HttpRequest.Body, and sets
                // the accepts & content-type header values to
                // application/xml
                _.Post
                    .Xml(new Input {Name = "Max", Age = 13})
                    .ToUrl("/person");

                // OR, if url lookup is enabled, this is an equivalent:
                _.Post.Xml(new Input {Name = "Max", Age = 13});
            });
        }
        // ENDSAMPLE

        public void customizing_serialization()
        {
            // SAMPLE: customizing-serialization
            var system = SystemUnderTest.ForStartup<Startup>();
            system.JsonSerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            // ENDSAMPLE

        }

        // SAMPLE: read-json
        public async Task read_json(IScenarioRunner system)
        {
            var result = await system.Scenario(_ =>
            {
                _.Get.Url("/output");
            });

            // This deserializes the response body to the
            // designated Output type
            var output = result.ResponseBody.ReadAsJson<Output>();

            // do assertions against the Output model
        }
        // ENDSAMPLE


        // SAMPLE: read-text
        public async Task read_text(IScenarioRunner system)
        {
            var result = await system.Scenario(_ =>
            {
                _.Get.Url("/output");
            });

            // This deserializes the response body to the
            // designated Output type
            var outputString = result.ResponseBody.ReadAsText();

            // do assertions against the Output string
        }
        // ENDSAMPLE

        // SAMPLE: assert-on-text
        public Task assert_on_content(IScenarioRunner system)
        {
            return system.Scenario(_ =>
            {
                _.ContentShouldBe("exactly this");

                _.ContentShouldContain("some snippet");

                _.ContentShouldNotContain("some warning");
            });
        }
        // ENDSAMPLE


        // SAMPLE: send-text
        public Task send_text(IScenarioRunner system)
        {
            return system.Scenario(_ =>
            {
                _.Post.Text("some text").ToUrl("/textdata");
            });
        }
        // ENDSAMPLE



        // SAMPLE: read-xml
        public async Task read_xml(IScenarioRunner system)
        {
            var result = await system.Scenario(_ =>
            {
                _.Get.Url("/output");
            });

            // This deserializes the response body to the
            // designated Output type
            var output = result.ResponseBody.ReadAsXml<Output>();

            // do assertions against the Output model

            // OR, if you just want the XmlDocument itself:
            XmlDocument document = result.ResponseBody.ReadAsXml();
        }
        // ENDSAMPLE
    }


    public class Input
    {
        public string Name;
        public int Age;
    }

    public class Output
    {
        public string Name;
        public int Age;
    }
}