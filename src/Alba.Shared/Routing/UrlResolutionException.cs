using System;
using System.Runtime.Serialization;

namespace Alba.Routing
{
    [Serializable]
    public class UrlResolutionException : Exception
    {
        public UrlResolutionException()
        {
        }

        public UrlResolutionException(string message) : base(message)
        {
        }

        public UrlResolutionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UrlResolutionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}