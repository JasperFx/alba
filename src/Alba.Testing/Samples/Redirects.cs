using System.Threading.Tasks;

namespace Alba.Testing.Samples
{
    public class Redirects
    {
        // SAMPLE: asserting-redirects
        public Task asserting_redirects(ISystemUnderTest system)
        {
            return system.Scenario(_ =>
            {
                // should redirect to the url
                _.RedirectShouldBe("/redirect");

                // should redirect permanently to the url
                _.RedirectPermanentShouldBe("/redirect");
            });
        }
        // ENDSAMPLE
    }
}
