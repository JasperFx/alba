using System.Collections.Generic;
using Alba.StaticFiles;
using Baseline.Testing;
using Shouldly;
using Xunit;

namespace Alba.Testing.StaticFiles
{
    public class WriteFileHeadContinuationTests
    {
        private readonly IDictionary<string, object> theEnvironment = new Dictionary<string, object>();
        private readonly AssetSettings theSettings = new AssetSettings();
        private StaticFile theFile;

        public WriteFileHeadContinuationTests()
        {
            new FileSystem().WriteStringToFile("lib.js", "var x = 0;");

            theFile = new StaticFile("lib.js");
            new WriteFileHeadContinuation(theEnvironment, theFile, 200).Write(theEnvironment);
        }

        [Fact]
        public void writes_the_content_length()
        {
            theEnvironment.ResponseHeaders().ContentLength()
                .ShouldBe(theFile.Length());
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