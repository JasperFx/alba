using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Baseline;
using Microsoft.AspNetCore.Http;

namespace Alba
{

    public static class FormDataExtensions
    {
        public static void WriteFormData(this HttpContext context,
            Dictionary<string, string> values)
        {
            var post = formData(values).Join("&");

            context.Request.ContentLength = post.Length;

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
    }
}