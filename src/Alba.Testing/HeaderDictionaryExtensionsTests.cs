using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class HeaderDictionaryExtensionsTests
    {
        private readonly IDictionary<string, string[]> theHeaders = new Dictionary<string, string[]>();
            
            
            
        [Fact]
        public void get_single_header_miss()
        {
            theHeaders.Get("foo").ShouldBeNull();
        }

        [Fact]
        public void get_single_header_hit()
        {
            theHeaders.Add("foo", new []{"bar"});

            theHeaders.Get("foo").ShouldBe("bar");
        }

        [Fact]
        public void get_single_header_hit_with_case_insensitive_search()
        {
            theHeaders.Add("foo", new[] { "bar" });

            theHeaders.Get("Foo").ShouldBe("bar");
        }

        [Fact]
        public void append_from_initial_state()
        {
            theHeaders.Append("foo", "bar");
            theHeaders["foo"].ShouldHaveTheSameElementsAs("bar");
        }

        [Fact]
        public void append_a_second_value()
        {
            theHeaders.Append("foo", "bar");
            theHeaders.Append("foo", "baz");

            theHeaders.Append("foo", "one", "two");

            theHeaders["foo"].ShouldHaveTheSameElementsAs("bar", "baz", "one", "two");
        }

        [Fact]
        public void replace_from_scratch()
        {
            theHeaders.Replace("foo", "bar");
            theHeaders["foo"].ShouldHaveTheSameElementsAs("bar");
        }

        [Fact]
        public void replace_with_existing_value()
        {
            theHeaders["foo"] = new[] {"bar"};

            theHeaders.Replace("foo", "coffee");

            theHeaders["foo"].ShouldHaveTheSameElementsAs("coffee");
        }

        [Fact]
        public void has_is_case_insensitive()
        {
            theHeaders.Has("foo").ShouldBeFalse();
            theHeaders.Has("Foo").ShouldBeFalse();


            theHeaders.Add("Foo", new []{"bar"});

            theHeaders.Has("foo").ShouldBeTrue();
            theHeaders.Has("Foo").ShouldBeTrue();
        }

        [Fact]
        public void get_all_is_case_insensitive()
        {
            theHeaders.Append("foo", "bar");
            theHeaders.Append("foo", "baz");

            theHeaders.Append("foo", "one", "two");

            theHeaders.GetAll("foo").ShouldHaveTheSameElementsAs("bar", "baz", "one", "two");
        }
    }
}