using System;

namespace AlbaRouter
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class UrlPatternAttribute : Attribute
    {
        public UrlPatternAttribute(string pattern)
        {
            Pattern = pattern.Trim();
        }

        public string Pattern { get; }
    }
}