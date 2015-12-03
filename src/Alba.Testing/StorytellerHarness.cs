using Shouldly;
using StoryTeller;
using StoryTeller.Engine;

namespace JasperRouter.Testing
{
    public class StorytellerHarness
    {
        public void run_spec()
        {
            using (var runner = new SpecRunner<NulloSystem>())
            {
                var results = runner.Run("Routing/Routing with Spread Arguments");

                runner.OpenResultsInBrowser();
                results.Counts.AssertSuccess();
            }
        } 
    }
}