using System.Threading.Tasks;

namespace Alba.Testing.Samples
{
    public class Urls
    {
        // SAMPLE: specify-the-url-directly
        public Task specify_url(AlbaTestHost system)
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


        // SAMPLE: reverse-url-lookup
        public Task reverse_url_lookup(AlbaTestHost system)
        {
            return system.Scenario(_ =>
            {
                // By controller action
                _.Get.Action<MyController>(x => x.Get());

                // By the action method's input model
                _.Post.Input(new InputModel {Id = "foo"});


                // Serializes the object passed in as Json,
                // writes that to the HttpRequest.Body,
                // and looks up the Url by the input model
                // type
                _.Put.Json(new InputModel());

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