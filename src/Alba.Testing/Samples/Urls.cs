using System.Threading.Tasks;

namespace Alba.Testing.Samples
{
    public class Urls
    {
        // SAMPLE: specify-the-url-directly
        public Task specify_url(AlbaHost system)
        {
            return system.Scenario(_ =>
            {
                // Directly specify the Url against a given
                // HTTP method
                _.Get.Url("/");
                _.Put.Url("/");
                _.Post.Url("/");
                _.Delete.Url("/");
                _.Head.Url("/");
            });
        }
        // ENDSAMPLE


    }

    public class InputModel
    {
        public string Id;
    }

    public class MyController
    {
        public string Get()
        {
            return "something";
        }
    }
}