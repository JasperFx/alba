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
                var results = runner.Run("Route Generation/Derive arguments from method signature");

                runner.OpenResultsInBrowser();
                results.Counts.AssertSuccess();
            }
        } 
    }
}