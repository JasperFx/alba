namespace Alba.Testing.Samples
{
    public class StatusCodes
    {
        #region sample_check_the_status_code
        public async Task check_the_status(IAlbaHost system)
        {
            await system.Scenario(_ =>
            {
                // Shorthand for saying that the StatusCode should be 200
                _.StatusCodeShouldBeOk();

                // Or a specific status code
                _.StatusCodeShouldBe(403);

                // Any status code between 200 and 299; this is the
                // default expectation for every scenario
                _.StatusCodeShouldBeSuccess();

                // Ignore the status code altogether
                _.IgnoreStatusCode();
            });
        }
        #endregion
    }
}
