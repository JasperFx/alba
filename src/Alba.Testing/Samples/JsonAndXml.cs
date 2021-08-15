using System;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;

namespace Alba.Testing.Samples
{
    public class JsonAndXml
    {
        #region sample_sending_json
        public Task send_json(IAlbaHost system)
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
        #endregion

        #region sample_sending_xml
        public Task send_xml(IAlbaHost system)
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
        #endregion

        #region sample_read_json
        public async Task read_json(IAlbaHost system)
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
        #endregion


        #region sample_read_text
        public async Task read_text(IAlbaHost system)
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
        #endregion

        #region sample_assert_on_text
        public Task assert_on_content(IAlbaHost system)
        {
            return system.Scenario(_ =>
            {
                _.ContentShouldBe("exactly this");

                _.ContentShouldContain("some snippet");

                _.ContentShouldNotContain("some warning");
            });
        }
        #endregion


        #region sample_send_text
        public Task send_text(IAlbaHost system)
        {
            return system.Scenario(_ =>
            {
                _.Post.Text("some text").ToUrl("/textdata");
            });
        }
        #endregion



        #region sample_read_xml
        public async Task read_xml(IAlbaHost system)
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
        #endregion
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
