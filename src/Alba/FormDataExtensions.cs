using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Baseline;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Primitives;

#if !NETCOREAPP3_0
using Microsoft.AspNetCore.Http.Internal;
#endif

namespace Alba
{

    public static class FormDataExtensions
    {
        /// <summary>
        /// Write the dictionary values to the HttpContext.Request.Body.
        /// Also sets content-length & content-type header to
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

        public static void WriteMultipartFormData(this HttpContext context, Dictionary<string, string> values, string filePath, string name)
        {
            var form = values.ToDictionary<KeyValuePair<string, string>, string, StringValues>(x => x.Key, x => x.Value);
            var formFileCollection = new FormFileCollection();
            var formFile = BuildFormFile(filePath, name);

            formFileCollection.Add(formFile);

            var formCollection = new FormCollection(form, formFileCollection);

            context.Request.Form = formCollection;
        }

        private static FormFile BuildFormFile(string filePath, string name)
        {
            var physicalFile = new FileInfo(filePath);
            var fileName = physicalFile.Name;

            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(physicalFile.OpenRead());
            writer.Flush();
            ms.Position = 0;

            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType);

            return new FormFile(ms, 0, ms.Length, name, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType,
                ContentDisposition = new ContentDispositionHeaderValue("form-data") {FileName = fileName, Name = name}.ToString()
            };
        }
    }
}