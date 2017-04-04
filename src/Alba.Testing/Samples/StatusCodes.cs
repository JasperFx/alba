using System.Threading.Tasks;

namespace Alba.Testing.Samples
{
    public class StatusCodes
    {
        // SAMPLE: check-the-status-code
        public Task check_the_status(ISystemUnderTest system)
        {
            return system.Scenario(_ =>
            {
                // Shorthand for saying that the StatusCode should be 200
                _.StatusCodeShouldBeOk();

                // Or a specific status code
                _.StatusCodeShouldBe(403);

                // Ignore the status code altogether
                _.IgnoreStatusCode();
            });
        }
        // ENDSAMPLE
    }
}