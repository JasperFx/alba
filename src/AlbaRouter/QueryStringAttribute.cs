using System;

namespace AlbaRouter
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class QueryStringAttribute : Attribute
    {
    }
}
