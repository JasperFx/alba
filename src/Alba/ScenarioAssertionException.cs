using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Baseline;
using Microsoft.AspNetCore.Http;

#if NET46
using System.Runtime.Serialization;
#endif

namespace Alba
{
#if NET46
    [Serializable]
#endif
    public class ScenarioAssertionException : Exception
    {
        private readonly IList<string> _messages = new List<string>();

        public ScenarioAssertionException()
        {
        }

#if NET46
        protected ScenarioAssertionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
        /// <summary>
        /// Add an assertion failure message
        /// </summary>
        /// <param name="message"></param>
        public void Add(string message)
        {
            _messages.Add(message);
        }

        public void AssertAll()
        {
            if (_messages.Any())
            {
                throw this;
            }
        }

        public IEnumerable<string> Messages => _messages;

        public override string Message
        {
            get
            {
                var writer = new StringWriter();
                _messages.Each(x => writer.WriteLine((string) x));

                if (Body.IsNotEmpty())
                {
                    writer.WriteLine();
                    writer.WriteLine();
                    writer.WriteLine("Actual body text was:");
                    writer.WriteLine();
                    writer.WriteLine(Body);
                }

                return writer.ToString();
            }
        }

        public string Body { get; set; }

        public string ReadBody(HttpContext context)
        {
            var stream = context.Response.Body;
            if (Body == null)
            {
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }
                
                Body = Encoding.UTF8.GetString(stream.ReadAllBytes());
            }

            return Body;
        }

        public void ShowActualBodyInErrorMessage(HttpContext context)
        {
            ReadBody(context);
        }
    }
}