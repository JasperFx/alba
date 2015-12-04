namespace Alba
{
    public class MissingThings
    {
        // ON RESPONSE
        /*
        public static IEnumerable<Cookie> Cookies(this OwinEnvironment env)
        {
            return HeaderValueFor(HttpResponseHeaders.SetCookie).Select(CookieParser.ToCookie);
        }

        public Cookie CookieFor(string name)
        {
            return Cookies().FirstOrDefault(x => x.Matches(name));
        }


        public bool ContentTypeMatches(MimeType mimeType)
        {
            return HeaderValueFor(HttpResponseHeaders.ContentType).Any(x => x.EqualsIgnoreCase(mimeType.Value));
        }
        */


        /*
        public static class CurrentHttpRequestExtensions
        {


        }
        */

        /*
    public class HttpRequestBody
    {
        private readonly Owinheadersironment _parent;

        public HttpRequestBody(Owinheadersironment parent)
        {
            _parent = parent;
        }

        public void XmlInputIs(object target)
        {
            var serializer = new XmlSerializer(target.GetType());
            serializer.Serialize(_parent.Input(), target);
            _parent.Input().Position = 0;
        }

        public void JsonInputIs(object target)
        {
            string json = null;

            if (_parent.ContainsKey("scenario-support"))
            {
                var serializer = _parent["scenario-support"]
                    .As<IScenarioSupport>()
                    .Get<IJsonSerializer>();

                json = serializer.Serialize(target);
            }
            else
            {
                json = JsonUtil.ToJson(target);
            }

            JsonInputIs(json);
        }

        public void JsonInputIs(string json)
        {
            var writer = new StreamWriter(_parent.Input());
            writer.Write(json);
            writer.Flush();

            _parent.Input().Position = 0;
        }

        public void WriteFormData<T>(T target) where T : class
        {
            new TypeDescriptorCache().ForEachProperty(typeof(T), prop =>
            {
                var rawValue = prop.GetValue(target, null);
                var httpValue = rawValue == null ? string.Empty : rawValue.ToString().UrlEncoded();

                _parent.Form()[prop.Name] = httpValue;
            });
        }

        public void ReplaceBody(Stream stream)
        {
            stream.Position = 0;
            _parent.Append(OwinConstants.RequestBodyKey, stream);
        }

    }
    */
    }
}