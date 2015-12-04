using System;
using System.Collections.Generic;
using System.IO;
using Alba.StaticFiles;
using Baseline.Testing;
using Shouldly;
using Xunit;

namespace Alba.Testing.StaticFiles
{
    public class FubuFileTests
    {
        public FubuFileTests()
        {
            new FileSystem().WriteStringToFile("a.txt", "some text from a.txt");
        }

        [Fact]
        public void read_contents()
        {
            var file = new StaticFile("a.txt");
            file.ReadContents().Trim().ShouldBe("some text from a.txt");
        }

        [Fact]
        public void read_lines()
        {
            var lines = new List<string>();
            Action<string> action = x => lines.Add(x);

            var file = new StaticFile("a.txt");

            file.ReadLines(action);

            lines.ShouldContain("some text from a.txt");
        }

        [Fact]
        public void read_contents_by_stream()
        {
            var wasCalled = false;
            var file = new StaticFile("a.txt");
            file.ReadContents(stream =>
            {
                wasCalled = true;
                stream.ReadAllText().ShouldBe("some text from a.txt");
            });

            wasCalled.ShouldBeTrue();
        }

        [Fact]
        public void length()
        {
            new FileSystem().WriteStringToFile("ghostbusters.txt", "Who you gonna call?");

            new StaticFile("ghostbusters.txt")
                .Length().ShouldBe(19);
        }

        [Fact]
        public void last_modified()
        {
            var now = DateTime.UtcNow;

            new FileSystem().WriteStringToFile("ghostbusters.txt", "Who you gonna call?");

            var lastModified = new StaticFile("ghostbusters.txt")
                .LastModified();


            (lastModified.ToFileTimeUtc() - now.ToFileTimeUtc())
                .ShouldBeLessThan(1);
        }

        [Fact]
        public void etag_is_predictable()
        {
            new FileSystem().WriteStringToFile("ghostbusters.txt", "Who you gonna call?");

            var etag1 = new StaticFile("ghostbusters.txt").Etag();
            var etag2 = new StaticFile("ghostbusters.txt").Etag();
            var etag3 = new StaticFile("ghostbusters.txt").Etag();

            etag1.ShouldBe(etag2);
            etag1.ShouldBe(etag3);
        }

        [Fact]
        public void etag_changes_on_file_changes()
        {
            new FileSystem().WriteStringToFile("ghostbusters.txt", "Who you gonna call?");

            var etag1 = new StaticFile("ghostbusters.txt").Etag();

            new FileSystem().WriteStringToFile("ghostbusters.txt", "He slimed me!");

            var etag2 = new StaticFile("ghostbusters.txt").Etag();

            etag1.ShouldNotBe(etag2);
        }
    }


}