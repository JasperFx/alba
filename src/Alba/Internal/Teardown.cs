namespace Alba.Internal;

internal static class Teardown
{
    // Every step runs even if an earlier one fails, and the failures are reported together
    public static async Task RunAllAsync(string failureMessage, IEnumerable<Func<Task>> steps)
    {
        List<Exception>? failures = null;
        foreach (var step in steps)
        {
            try
            {
                await step();
            }
            catch (Exception e)
            {
                (failures ??= new List<Exception>()).Add(e);
            }
        }

        if (failures is not null)
        {
            throw new AggregateException(failureMessage, failures);
        }
    }
}
