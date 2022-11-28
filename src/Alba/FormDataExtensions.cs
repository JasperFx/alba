using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Baseline;
using Microsoft.AspNetCore.Http;

namespace Alba
{

    public static class FormDataExtensions
    {
        /// <summary>
        /// Write the dictionary values to the HttpContext.Request.Body.
        /// Also sets content-length and content-type header to
        /// application/x-www-form-urlencoded
        /// </summary>
        /// <param name="context"></param>
        /// <param name="values"></param>
        public static void WriteFormData(this HttpContext context,
            Dictionary<string, string> values)
        {
            var post = formData(values).Join("&");

            context.Request.ContentLength = post.Length;
            context.Request.ContentType = MimeType.HttpFormMimetype;

            var postBytes = Encoding.UTF8.GetBytes(post);

            var stream = new MemoryStream();
            stream.Write(postBytes, 0, postBytes.Length);
            stream.Position = 0;

            context.Request.Body = stream;
        }

        private static IEnumerable<string> formData(Dictionary<string, string> form)
        {
            foreach (var key in form.Keys)
            {
                yield return "{0}={1}".ToFormat(key, WebUtility.HtmlEncode(form[key]));
            }

        }
        
        /// <summary>
        /// Writes the <see cref="MultipartFormDataContent"/> to the provided HttpContext, along with the
        /// required headers.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="content"></param>
        public static void WriteMultipartFormData(this HttpContext context, MultipartFormDataContent content)
        {
           content.CopyTo(context.Request.Body, null, CancellationToken.None);
           foreach (var kv in content.Headers)
           {
               context.Request.Headers.Add(kv.Key, kv.Value.ToArray());
           }
        }
    }
}