using System;
using System.Collections.Generic;
using System.Text;

namespace Alba.Routing
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class QueryStringAttribute : Attribute
    {
    }
}
