using System;
using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class ContentNegotiationExtensionsTests
    {
        [Fact]
        public void modified_since()
        {
            var time = new DateTime(2014, 1, 30, 12, 5, 6);

            new Dictionary<string, object>()
                .IfModifiedSince(time)
                .IfModifiedSince()
                .ShouldBe(time.ToUniversalTime());
        }

        [Fact]
        public void un_modified_since()
        {
            var time = new DateTime(2014, 1, 30, 12, 5, 6);

            new Dictionary<string, object>()
                .IfUnModifiedSince(time)
                .IfUnModifiedSince()
                .ShouldBe(time.ToUniversalTime());
        }

        [Fact]
        public void if_match()
        {
            new Dictionary<string, object>().IfMatch("a,b, c")
                .IfMatch()
                .ShouldHaveTheSameElementsAs("a", "b", "c");
        }

        [Fact]
        public void if_none_match()
        {
            new Dictionary<string, object>().IfNoneMatch("a,b, c")
                .IfNoneMatch()
                .ShouldHaveTheSameElementsAs("a", "b", "c");
        }

        [Fact]
        public void etag_matches_with_no_values()
        {
            new string[0].EtagMatches("foo")
                .ShouldBe(EtagMatch.None);
        }

        [Fact]
        public void etag_matches_with_wildcard()
        {
            new string[] { "a", "*", "b" }
                .EtagMatches("foo")
                .ShouldBe(EtagMatch.Yes);
        }

        [Fact]
        public void etag_matches_positive()
        {
            new string[] { "a", "b", "foo" }
                .EtagMatches("foo")
                .ShouldBe(EtagMatch.Yes);
        }

        [Fact]
        public void etag_matches_negative()
        {
            new string[] { "a", "b", "bar" }
                .EtagMatches("foo")
                .ShouldBe(EtagMatch.No);
        }

    }
}