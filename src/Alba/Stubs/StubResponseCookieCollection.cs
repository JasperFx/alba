#if NETCOREAPP3_0
using Microsoft.AspNetCore.Http;

namespace Alba.Stubs
{
    public class StubResponseCookieCollection : IResponseCookies
    {
        public void Append(string key, string value)
        {
        }

        public void Append(string key, string value, CookieOptions options)
        {
        }

        public void Delete(string key)
        {
        }

        public void Delete(string key, CookieOptions options)
        {
        }
    }
}
#endif
