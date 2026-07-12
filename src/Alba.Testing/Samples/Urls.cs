namespace Alba.Testing.Samples
{
    public class Urls
    {
        #region sample_specify_the_url_directly
        public async Task specify_url(AlbaHost system)
        {
            await system.Scenario(_ =>
            {
                // Directly specify the Url against a given
                // HTTP method
                _.Get.Url("/");
                _.Put.Url("/");
                _.Post.Url("/");
                _.Delete.Url("/");
                _.Patch.Url("/");
                _.Head.Url("/");
                _.Query.Url("/");
            });
        }
        #endregion

        #region sample_query_string_parameters
        public async Task query_string_parameters(AlbaHost system)
        {
            await system.Scenario(_ =>
            {
                // Add individual query string parameters
                _.Get.Url("/search").QueryString("q", "alba").QueryString("page", "2");

                // Or append one parameter per public property or field
                // of an object
                _.Get.Url("/search").QueryString(new { q = "alba", page = 2 });
            });
        }
        #endregion


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
