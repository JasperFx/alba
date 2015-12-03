using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using Baseline.Testing;

namespace Alba
{

    public static class FormDataExtensions
    {
        public static NameValueCollection ParseFormData(this IDictionary<string, object> env)
        {
            if (env.ContainsKey(OwinConstants.RequestBodyKey))
            {
                var body = env[OwinConstants.RequestBodyKey].As<Stream>();
                var rawData = body.ReadAllText();
                return HttpUtility.ParseQueryString(rawData);
            }

            return new NameValueCollection();
        }

        public static IDictionary<string, object> WriteFormData(this IDictionary<string, object> env,
            NameValueCollection values)
        {
            var post = formData(values).Join("&");
            var postBytes = Encoding.Default.GetBytes(post);

            var stream = new MemoryStream();
            stream.Write(postBytes, 0, postBytes.Length);
            stream.Position = 0;
            env.Set(OwinConstants.RequestBodyKey, stream);

            return env;
        } 

        private static IEnumerable<string> formData(NameValueCollection form)
        {
            foreach (var key in form.AllKeys)
            {
                yield return "{0}={1}".ToFormat(key, HttpUtility.HtmlEncode(form[key]));
            }

        }
    }
}