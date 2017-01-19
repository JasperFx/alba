using System;

#if NET46
using System.Runtime.Serialization;

#endif

namespace AlbaRouter
{
#if NET46
    [Serializable]
#endif
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

#if NET46
        protected UrlResolutionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
#endif
    }
}