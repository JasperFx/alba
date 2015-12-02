using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using Baseline.Testing;

namespace Alba
{
    // TODO -- have this be "ParseForm()" instead
    public static class FormDataExtensions
    {
        public static NameValueCollection Form(this IDictionary<string, object> env)
        {

            if (!env.ContainsKey(OwinConstants.RequestFormKey))
            {
                env.Add(OwinConstants.RequestFormKey, new NameValueCollection());
            }

            return env.Get<NameValueCollection>(OwinConstants.RequestFormKey);
        }

        private static IEnumerable<string> formData(this IDictionary<string, object> env)
        {
            var form = env.Form();
            foreach (var key in form.AllKeys)
            {
                yield return "{0}={1}".ToFormat(key, HttpUtility.HtmlEncode(form[key]));
            }

        }

        // TODO -- this should go somewhere else
        public static void RewindData(this IDictionary<string, object> env)
        {
            if (env.ContainsKey(OwinConstants.RequestFormKey) && env.Form().Count > 0)
            {
                var post = env.formData().Join("&");
                var postBytes = Encoding.Default.GetBytes(post);
                env.Input().Write(postBytes, 0, postBytes.Length);

                env.Remove(OwinConstants.RequestFormKey);
            }

            if (env.ContainsKey(OwinConstants.RequestBodyKey))
            {
                env.Input().Position = 0;
            }
            else
            {
                env.Add(OwinConstants.ResponseBodyKey, new MemoryStream());
            }
        }
    }
}