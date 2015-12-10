using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using Baseline;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class FormDataExtensionsTests
    {
        [Fact]
        public void read_form_data()
        {
            var env = new Dictionary<string, object>();


            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write("a=1&b=2&c=3");
            writer.Flush();
            stream.Position = 0;

            env.Add(OwinConstants.RequestBodyKey, stream);

            var form = env.ParseFormData();

            form.AllKeys.ShouldHaveTheSameElementsAs("a", "b", "c");

            form["a"].ShouldBe("1");
            form["b"].ShouldBe("2");
            form["c"].ShouldBe("3");
        }

        [Fact]
        public void round_trip_writing_and_parsing()
        {
            var form1 = new NameValueCollection();
            form1["a"] = "what?";
            form1["b"] = "now?";
            form1["c"] = "really?";

            var env = new Dictionary<string, object>();

            var form2 = env.WriteFormData(form1).ParseFormData();

            form2.AllKeys.ShouldHaveTheSameElementsAs(form1.AllKeys);
            form2.AllKeys.Each(key =>
            {
                form2[key].ShouldBe(form1[key]);
            });
        }


    }
}