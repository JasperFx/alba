using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Alba.Testing.Samples
{
    public class Headers
    {
        // SAMPLE: conneg-helpers
        public Task conneg_helpers(IScenarioRunner system)
        {
            return system.Scenario(_ =>
            {
                // Set the accepts header on the request
                _.Get.Url("/").Accepts("text/plain");

                // Specify the etag header value
                _.Get.Url("/").Etag("12345");

                // Set the content-type header on the request
                _.Post.Url("/").ContentType("text/json");

                // This is a superset of the code above that
                // will set the content-type header as well
                _.Post.Json(new InputModel()).ToUrl("/");
            });
        }
        // ENDSAMPLE

        // SAMPLE: setting-request-headers
        public Task setting_request_headers(IScenarioRunner system)
        {
            return system.Scenario(_ =>
            {
                _.SetRequestHeader("foo", "bar");
                
            });
        }
        // ENDSAMPLE


        // SAMPLE: asserting-on-header-values
        public Task asserting_on_header_values(IScenarioRunner system)
        {
            return system.Scenario(_ =>
            {
                // Assert that there is one and only one value equal to "150"
                _.Header("content-length").SingleValueShouldEqual("150");

                // Assert that there is no value for this response header
                _.Header("connection").ShouldNotBeWritten();

                // Only write one value for this header
                _.Header("set-cookie").ShouldHaveOneNonNullValue();

                // Assert that the header has the given values
                _.Header("www-authenticate").ShouldHaveValues("NTLM", "Negotiate");

                // Assert that the header matches a regular expression
                _.Header("location").SingleValueShouldMatch(new Regex(@"^/items/\d*$"));

                // Check the content-type header
                _.ContentTypeShouldBe("text/json");
            });
        }
        // ENDSAMPLE
    }
}