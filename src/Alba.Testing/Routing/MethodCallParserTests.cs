using System.Collections.Generic;
using Alba.Routing;
using Alba.Testing.Urls;
using Alba.Urls;
using Shouldly;
using Xunit;

namespace Alba.Testing.Routing
{
    public class MethodCallParserTests
    {
        [Fact]
        public void resolve_properties_for_one_parameter_passed_as_constant()
        {
            MethodCallParser.ToArguments<FakeEndpoint>(x => x.do_stuff("something"))
                .ShouldHaveTheSameElementsAs("something");
        }

        [Fact]
        public void resolve_properties_for_one_parameter_passed_as_field()
        {
            var val = "nothing";
            MethodCallParser.ToArguments<FakeEndpoint>(x => x.do_stuff(val))
                .ShouldHaveTheSameElementsAs("nothing");
        }

        [Fact]
        public void resolve_properties_for_one_parameter_passed_as_method_arg()
        {
            var val = "other";
            MethodCallParser.ToArguments<FakeEndpoint>(x => x.do_stuff(val))
                .ShouldHaveTheSameElementsAs("other");
        }

        private List<object> fetch(string value)
        {
            return MethodCallParser.ToArguments<FakeEndpoint>(x => x.do_stuff(value));
        }


        [Fact]
        public void resolve_properties_for_multiple_parameters()
        {
            MethodCallParser.ToArguments<FakeEndpoint>(x => x.complex("else", 3))
                .ShouldHaveTheSameElementsAs("else", 3);
        }
    }

    public class FakeEndpoint
    {
        public void simple() { }


        public void do_stuff(string key)
        {

        }

        public void complex(string key, int number)
        {

        }
    }
}