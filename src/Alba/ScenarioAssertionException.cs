using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Alba.Internal;
using Microsoft.AspNetCore.Http;

 
namespace Alba
{

    public class ScenarioAssertionException : Exception
    {
        private readonly IList<string> _messages = new List<string>();

        public ScenarioAssertionException()
        {
        }

        /// <summary>
        /// Add an assertion failure message
        /// </summary>
        /// <param name="message"></param>
        public void Add(string message)
        {
            _messages.Add(message);
        }

        internal void AssertAll()
        {
            if (_messages.Any())
            {
                throw this;
            }
        }

        /// <summary>
        /// All the assertion failure messages
        /// </summary>
        public IEnumerable<string> Messages => _messages;

        public override string Message
        {
            get
            {
                var writer = new StringWriter();

                foreach (var message in _messages)
                {
                    writer.WriteLine(message);
                }

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

        /// <summary>
        /// A textual representation of the HTTP response body for diagnostic purposes. This property will be null unless <see cref="ReadBody"/> is called.
        /// </summary>
        public string? Body { get; set; }

        /// <summary>
        /// Reads the response body and returns it as a string
        /// </summary>
        /// <param name="context">The context of the response</param>
        /// <returns>A string with the content of the body</returns>
        [MemberNotNull(nameof(Body))]
        public string ReadBody(HttpContext context)
        {
            // Hardening for GH-95
            try
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
            }
            catch (Exception)
            {
                Body = string.Empty;
            }

            return Body;
        }
    }
}