using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class OwinRequestExtensionsTests
    {
        [Fact]
        public void read_request_body_as_text()
        {
            var text = "some text written up as part of the request body";

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;

            var env = new Dictionary<string, object>();
            env.Add(OwinConstants.RequestBodyKey, stream);

            env.ReadRequestBodyAsText().ShouldBe(text);
        }

        [Fact]
        public void has_body_data_positive()
        {
            var text = "some text written up as part of the request body";

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;

            var env = new Dictionary<string, object>();
            env.Add(OwinConstants.RequestBodyKey, stream);

            env.HasRequestBody().ShouldBeTrue();
        }



        [Fact]
        public void request_headers_when_none()
        {
            var dict = new Dictionary<string, object>();
            var headers = dict.RequestHeaders();
            headers.ShouldNotBeNull();

            dict[OwinConstants.RequestHeadersKey].ShouldBeSameAs(headers);
        }

        [Fact]
        public void existing_request_headers()
        {
            var dict = new Dictionary<string, object>();
            var headers = new Dictionary<string, string[]>();
            dict.Add(OwinConstants.RequestHeadersKey, headers);

            dict.RequestHeaders().ShouldBeSameAs(headers);
        }

        [Fact]
        public void get_request_body()
        {
            var dict = new Dictionary<string, object>();
            dict.HasRequestBody().ShouldBeFalse();

            dict.RequestBody().ShouldBeNull();

            var stream = new MemoryStream();

            dict.Add(OwinConstants.RequestBodyKey, stream);

            dict.RequestBody().ShouldBeSameAs(stream);
        }

        [Fact]
        public void is_client_connected_with_cancellation_token_still_connected()
        {
            var dict = new Dictionary<string, object>();
            var cancellation = new CancellationToken();
            cancellation.IsCancellationRequested.ShouldBeFalse();

            dict.Add(OwinConstants.CallCancelledKey, cancellation);

            dict.IsClientConnected().ShouldBeTrue();
        }

        [Fact]
        public void is_client_connected_with_cancellation_requested()
        {
            var dict = new Dictionary<string, object>();
            var source = new CancellationTokenSource();
            source.Cancel();

            dict.Set(OwinConstants.CallCancelledKey, source.Token);

            dict.IsClientConnected().ShouldBeFalse();

        }
    }

}