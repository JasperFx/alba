using System;

namespace Alba
{
    public class AlbaJsonFormatterException : Exception
    {
        public AlbaJsonFormatterException(IScenarioResult result) : base("The JSON formatter was unable to process the raw JSON:\n" + result.ReadAsText())
        {
        }
    }
}