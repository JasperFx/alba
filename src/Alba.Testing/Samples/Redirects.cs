using System.Threading.Tasks;

namespace Alba.Testing.Samples
{
    public class Redirects
    {
        #region sample_asserting_redirects
        public Task asserting_redirects(IAlbaHost system)
        {
            return system.Scenario(_ =>
            {
                // should redirect to the url
                _.RedirectShouldBe("/redirect");

                // should redirect permanently to the url
                _.RedirectPermanentShouldBe("/redirect");
            });
        }
        #endregion
    }
}
