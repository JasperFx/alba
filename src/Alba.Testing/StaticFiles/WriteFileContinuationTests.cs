using System.Collections.Generic;
using Alba.StaticFiles;
using Baseline.Testing;
using Shouldly;
using Xunit;

namespace Alba.Testing.StaticFiles
{
    public class WriteFileContinuationTests
    {
        private readonly IDictionary<string, object> theEnvironment = new Dictionary<string, object>();
        private readonly AssetSettings theSettings = new AssetSettings();
        private StaticFile theFile;

        public WriteFileContinuationTests()
        {
            new FileSystem().WriteStringToFile("top.js", "var x = 0;");

            theFile = new StaticFile("top.js");
            new WriteFileContinuation(theEnvironment, theFile, theSettings).Write(theEnvironment);
        }

        [Fact]
        public void writes_the_file_contents_to_the_response_body()
        {
            theEnvironment.Response().ReadAsText().ShouldBe("var x = 0;");
        }

        [Fact]
        public void writes_the_derived_content_type()
        {
            theEnvironment.ResponseHeaders().ContentType().ShouldBe("application/javascript");
        }

        [Fact]
        public void writes_200_status_code()
        {
            theEnvironment.StatusCode().ShouldBe(200);
        }

        [Fact]
        public void writes_the_headers_from_asset_settings()
        {
            var headers = theEnvironment.ResponseHeaders();
            headers.Has(HttpGeneralHeaders.CacheControl).ShouldBeTrue();
            headers.Has(HttpGeneralHeaders.Expires).ShouldBeTrue();
        }

        [Fact]
        public void writes_the_content_length()
        {
            theEnvironment.ResponseHeaders().ContentLength()
                .ShouldBe(theFile.Length());
        }

        [Fact]
        public void writes_the_last_modified_header()
        {
            theEnvironment.ResponseHeaders().Get(HttpGeneralHeaders.LastModified)
                .ShouldBe(theFile.LastModified().ToString("r"));
        }

        [Fact]
        public void writes_the_etag_for_the_file()
        {
            theEnvironment.ResponseHeaders().Get(HttpResponseHeaders.ETag)
                .ShouldBe(theFile.Etag().Quoted());
        }
    }
}