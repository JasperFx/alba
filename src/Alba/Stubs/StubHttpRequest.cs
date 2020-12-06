using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Alba.Stubs
{
    public class StubHttpRequest : HttpRequest
    {
        private readonly FormFeature _formFeature;

        public StubHttpRequest(HttpContext context)
        {
            HttpContext = context;
            _formFeature = new FormFeature(this);

            Query = new QueryCollection(_queryStringValues);
        }   

        public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _formFeature.ReadFormAsync(cancellationToken);
        }

        public override HttpContext HttpContext { get; }
        public override string Method { get; set; } = "GET";
        public override string Scheme { get; set; } = "http";
        public override bool IsHttps { get; set; } = false;
        public override HostString Host { get; set; } = new HostString("localhost", 5000);
        public override PathString PathBase { get; set; } = "/";
        public override PathString Path { get; set; } = "/";
        public override QueryString QueryString { get; set; } = QueryString.Empty;
        public override IQueryCollection Query { get; set; } 

        private readonly Dictionary<string, StringValues> _queryStringValues = new Dictionary<string, StringValues>();


        public override string Protocol { get; set; } = default!;
        public override IHeaderDictionary Headers { get; } = new HeaderDictionary();
        public override IRequestCookieCollection Cookies { get; set; } = new StubRequestCookieCollection();


        public override long? ContentLength
        {
            get => Headers.ContentLength();
            set => Headers.ContentLength(value);
        }

        public override string ContentType
        {
            get => Headers[HeaderNames.ContentType];
            set => Headers[HeaderNames.ContentType] = value;
        }

        public override Stream Body { get; set; } = new MemoryStream();

        public override bool HasFormContentType => _formFeature.HasFormContentType;

        public override IFormCollection Form
        {
            get => _formFeature.ReadForm();
            set => _formFeature.Form = value;
        }

        public void AddQueryString(string paramName, string paramValue)
        {
            QueryString = QueryString.Add(paramName, paramValue);

            if (_queryStringValues.ContainsKey(paramName))
            {
                _queryStringValues[paramName] = paramValue;
            }
            else
            {
                _queryStringValues.Add(paramName, paramValue);
            }
        }
    }
}