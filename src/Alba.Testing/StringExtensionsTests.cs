using System;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class StringExtensionsTests
    {
        [Fact]
        public void get_comma_separated_values_from_header()
        {
            new[] { "v1", "v2, v3", "\"v4, b\"", "v5, v6", "v7", }
                .GetCommaSeparatedHeaderValues()
                .ShouldHaveTheSameElementsAs("v1", "v2", "v3", "v4, b", "v5", "v6", "v7");

            new[] { "v1,v2, v3,\"v4, b\",v5, v6,v7" }
                .GetCommaSeparatedHeaderValues()
                .ShouldHaveTheSameElementsAs("v1", "v2", "v3", "v4, b", "v5", "v6", "v7");
        }

        [Fact]
        public void quoted_string()
        {
            "foo".Quoted().ShouldBe("\"foo\"");
        }


        [Fact]
        public void try_parse_http_date()
        {
            var date = DateTime.Today.AddHours(3);

            var datestring = date.ToString("r");

            datestring.TryParseHttpDate().ShouldBe(date);
        }

        [Fact]
        public void try_parse_http_date_with_empty_string()
        {
            "".TryParseHttpDate().ShouldBeNull();
        }


    }
}