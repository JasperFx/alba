using Alba.StaticFiles;
using Shouldly;
using Xunit;

namespace Alba.Testing.StaticFiles
{
    public class AssetSettingsTests
    {
        [Fact]
        public void out_of_the_box_headers()
        {
            var settings = new AssetSettings();
            settings.Headers.GetAllKeys()
                .ShouldHaveTheSameElementsAs(HttpGeneralHeaders.CacheControl, HttpGeneralHeaders.Expires);

            settings.Headers[HttpGeneralHeaders.CacheControl]().ShouldBe("private, max-age=86400");
            settings.Headers[HttpGeneralHeaders.Expires]().ShouldNotBeNull();
        } 
    }
}