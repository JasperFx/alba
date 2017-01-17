using System;

namespace Alba.Routing
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class UrlPatternAttribute : Attribute
    {
        private readonly string _pattern;

        public UrlPatternAttribute(string pattern)
        {
            _pattern = pattern.Trim();
        }
    }
}