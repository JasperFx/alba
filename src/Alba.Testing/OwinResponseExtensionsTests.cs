using System.Collections.Generic;
using System.IO;
using System.Linq;
using Baseline.Testing;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class OwinResponseExtensionsTests
    {
        [Fact]
        public void set_and_read_request_id()
        {
            var dict = new Dictionary<string, object>();
            dict.RequestId().ShouldBeNull();

            dict.RequestId("abc");

            dict.RequestId().ShouldBe("abc");

            dict.ResponseHeaders()[OwinConstants.REQUEST_ID].Single().ShouldBe("abc");
        }


        [Fact]
        public void response_headers_when_none()
        {
            var dict = new Dictionary<string, object>();
            var headers = dict.ResponseHeaders();
            headers.ShouldNotBeNull();

            dict[OwinConstants.ResponseHeadersKey].ShouldBeSameAs(headers);
        }

        [Fact]
        public void existing_response_headers()
        {
            var dict = new Dictionary<string, object>();
            var headers = new Dictionary<string, string[]>();
            dict.Add(OwinConstants.ResponseHeadersKey, headers);

            dict.ResponseHeaders().ShouldBeSameAs(headers);
        }

        [Fact]
        public void create_response_stream_on_the_fly_if_one_does_not_already_exist()
        {
            var env = new Dictionary<string, object>();

            var stream = env.ResponseStream();
            stream.ShouldBeOfType<MemoryStream>();

            env[OwinConstants.ResponseBodyKey].ShouldBeSameAs(stream);
        }

        [Fact]
        public void get_existing_body()
        {
            var env = new Dictionary<string, object>();
            var stream = new MemoryStream();
            env.Add(OwinConstants.ResponseBodyKey, stream);

            env.ResponseStream().ShouldBeSameAs(stream);
        }

        [Fact]
        public void write_a_file_without_sendfile_key()
        {
            var fileSystem = new FileSystem();
            var contents = "some text in a file";
            fileSystem.WriteStringToFile("content.txt", contents);

            var env = new Dictionary<string, object>();
            env.WriteFile("content.txt");

            env.ResponseHeaders().ContentLength().ShouldBe(contents.Length);
            var body = env.ResponseStream();
            body.Position = 0;

            body.ReadAllText().ShouldBe(contents);
        }

        [Fact]
        public void status_code_round_trip()
        {
            var env = new Dictionary<string, object>();
            env.StatusCode(406);
            env.StatusCode().ShouldBe(406);
            env[OwinConstants.ResponseStatusCodeKey].ShouldBe(406);
        }

        [Fact]
        public void status_description_round_trip()
        {
            var env = new Dictionary<string, object>();
            env.StatusDescription("I did not like it");

            env.StatusDescription().ShouldBe("I did not like it");
        }

        [Fact]
        public void write_to_response_body()
        {
            var env = new Dictionary<string, object>();
            env.Write(x =>
            {
                var writer = new StreamWriter(x);
                writer.Write("Something");
                writer.Flush();
            });

            env.Response().ReadAsText().ShouldBe("Something");
        }

        [Fact]
        public void write_string_content_directory_to_response()
        {
            var contents = "some contents of the body";

            var env = new Dictionary<string, object>();
            env.Write(contents, "text/plain");

            env.Response().ReadAsText().ShouldBe(contents);
            env.ResponseHeaders().ContentType().ShouldBe("text/plain");
        }

        [Fact]
        public void redirect()
        {
            var env = new Dictionary<string, object>();
            env.Redirect("http://cnn.com");

            env.StatusCode().ShouldBe(302);
            env.ResponseHeaders().Get("Location").ShouldBe("http://cnn.com");
            env.Response().ReadAsText().ShouldContain("The document has moved");
        }
    }
}