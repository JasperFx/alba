namespace Alba;

public sealed class AlbaJsonFormatterException : Exception
{
    public AlbaJsonFormatterException(string json) : base($"The JSON formatter was unable to process the raw JSON:\n{json}")
    {
    }
}