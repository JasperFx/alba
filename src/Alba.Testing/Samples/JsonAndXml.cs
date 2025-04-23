using System.Xml;

namespace Alba.Testing.Samples
{
    public class JsonAndXml
    {
        #region sample_sending_json
        public async Task send_json(IAlbaHost host)
        {
            await host.Scenario(_ =>
            {
                // This serializes the Input object to json,
                // writes it to the HttpRequest.Body, and sets
                // the accepts & content-type header values to
                // application/json
                _.Post
                    .Json(new Input {Name = "Max", Age = 13})
                    .ToUrl("/person");
            });
        }
        
        public async Task send_json_minimal_api(IAlbaHost host)
        {
            await host.Scenario(_ =>
            {
                // In a system that has mixed Minimal API and MVC usage,
                // you may need to help Alba know if the route being tested
                // should use Minimal API or MVC Core compliant JSON testing
                _.Post
                    .Json(new Input {Name = "Max", Age = 13}, JsonStyle.MinimalApi)
                    .ToUrl("/person");
            });
        }
        #endregion


        #region sample_sending_xml
        public async Task send_xml(IAlbaHost host)
        {
            await host.Scenario(_ =>
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
        public async Task read_json(IAlbaHost host)
        {
            var result = await host.Scenario(_ =>
            {
                _.Get.Url("/output");
            });

            // This deserializes the response body to the
            // designated Output type
            var output = result.ReadAsJsonAsync<Output>();

            // do assertions against the Output model
        }
        #endregion
        
        #region sample_read_json_shorthand
        public async Task read_json_shorthand(IAlbaHost host)
        {
            var output = await host.GetAsJson<Output>("/output");

            // do assertions against the Output model
        }
        #endregion


        #region sample_read_text
        public async Task read_text(IAlbaHost host)
        {
            var result = await host.Scenario(_ =>
            {
                _.Get.Url("/output");
            });

            // This deserializes the response body to the
            // designated Output type
            var outputString = await result.ReadAsTextAsync();

            // do assertions against the Output string
        }
        #endregion

        #region sample_assert_on_text
        public async Task assert_on_content(IAlbaHost host)
        {
            await host.Scenario(_ =>
            {
                _.ContentShouldBe("exactly this");

                _.ContentShouldContain("some snippet");

                _.ContentShouldNotContain("some warning");
            });
        }
        #endregion


        #region sample_send_text
        public async Task send_text(IAlbaHost host)
        {
            await host.Scenario(_ =>
            {
                _.Post.Text("some text").ToUrl("/textdata");
            });
        }
        #endregion



        #region sample_read_xml
        public async Task read_xml(IAlbaHost host)
        {
            var result = await host.Scenario(_ =>
            {
                _.Get.Url("/output");
            });

            // This deserializes the response body to the
            // designated Output type
            var output = result.ReadAsXml<Output>();

            // do assertions against the Output model

            // OR, if you just want the XmlDocument itself:
            XmlDocument document = await result.ReadAsXmlAsync();
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
