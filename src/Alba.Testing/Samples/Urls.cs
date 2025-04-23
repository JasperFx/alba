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
                _.Head.Url("/");
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
