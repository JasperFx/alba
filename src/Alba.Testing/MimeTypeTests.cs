using Shouldly;

namespace Alba.Testing
{
    public class MimeTypeTests
    {
        [Fact]
        public void find_default_extension_for_javascript()
        {
            MimeType.Javascript.DefaultExtension().ShouldBe(".js");
        }

        [Fact]
        public void find_default_extension_for_css()
        {
            MimeType.Css.DefaultExtension().ShouldBe(".css");
        }

        [Fact]
        public void find_default_extension_for_truetype_font()
        {
            MimeType.TrueTypeFont.DefaultExtension().ShouldBe(".ttf");
        }

        [Fact]
        public void determine_mime_type_from_name_for_js()
        {
            MimeType.MimeTypeByFileName("file.coffee.js")
                .ShouldBe(MimeType.Javascript);
        }

        [Fact]
        public void determine_mime_type_from_name_for_css()
        {
            MimeType.MimeTypeByFileName("style.css")
                .ShouldBe(MimeType.Css);
        }

        [Fact]
        public void determine_mime_type_from_name_for_truetype_font()
        {
            MimeType.MimeTypeByFileName("somefont.ttf")
                .ShouldBe(MimeType.TrueTypeFont);
        }

        [Fact]
        public void determine_mime_type_for_an_extension_that_has_been_added()
        {
            MimeType.Javascript.AddExtension(".coffee");
            MimeType.Css.AddExtension(".scss");

            MimeType.MimeTypeByFileName("file.coffee").ShouldBe(MimeType.Javascript);
            MimeType.MimeTypeByFileName("file.scss").ShouldBe(MimeType.Css);
        }

        [Fact]
        public void can_return_null_for_a_totally_unrecognized_extension()
        {
            MimeType.MimeTypeByFileName("foo.657878XXXXXX")
                .ShouldBeNull();
        }

        [Fact]
        public void mimetype_from_extended_extension_set()
        {
            MimeType.MimeTypeByFileName("foo.323")
                .Value.ShouldBe("text/h323");
        }

        [Fact]
        public void lookups_are_safe_from_multiple_threads()
        {
            // The lookups cache misses into static state shared by every host
            var values = Enumerable.Range(0, 500).Select(i => $"application/vnd.alba-{i}").ToArray();

            Parallel.ForEach(values.Concat(values), value =>
            {
                MimeType.MimeTypeByValue(value).Value.ShouldBe(value);
                MimeType.MimeTypeByFileName("foo.js")!.Value.ShouldBe("application/javascript");
            });

            var all = MimeType.All().Select(x => x.Value).ToArray();
            values.Where(x => !all.Contains(x)).ShouldBeEmpty();
        }
    }
}