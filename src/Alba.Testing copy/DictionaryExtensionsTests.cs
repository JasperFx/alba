using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class DictionaryExtensionsTests
    {
        [Fact]
        public void copy_multiple_keys_that_all_exist_in_source()
        {
            var source = new Dictionary<string, object>();
            source.Add("a", 1);
            source.Add("b", 2);
            source.Add("c", 3);

            var destination = new Dictionary<string, object>();

            source.CopyTo(destination, "a", "b", "c");

            destination["a"].ShouldBe(1);
            destination["b"].ShouldBe(2);
            destination["c"].ShouldBe(3);
        }

        [Fact]
        public void copy_multiple_keys_with_some_misses()
        {
            var source = new Dictionary<string, object>();
            source.Add("a", 1);
            //source.Add("b", 1);
            source.Add("c", 3);

            var destination = new Dictionary<string, object>();

            source.CopyTo(destination, "a", "b", "c");

            destination["a"].ShouldBe(1);
            destination.ContainsKey("b").ShouldBeFalse();
            destination["c"].ShouldBe(3);
        }
    }
}