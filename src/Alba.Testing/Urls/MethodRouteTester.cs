using System.Collections.Generic;
using Alba.Urls;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Alba.Testing.Urls
{
    public class MethodRouteTester
    {
        [Fact]
        public void has_parameters_false_on_method_with_empty_params()
        {
            MethodRoute<FakeEndpoint>.For(x => x.simple(), "simple", "GET")
                .HasParameters.ShouldBeFalse();

        }

        [Fact]
        public void has_parameters_true_for_method_with_route_arguments()
        {
            var method = MethodRoute<FakeEndpoint>.For(x => x.do_stuff(null), "stuff/:key", "GET");
            method.AddParameter("key");

            method.HasParameters.ShouldBeTrue();
        }

        [Fact]
        public void resolve_properties_for_one_parameter_passed_as_variable()
        {
            var method = MethodRoute<FakeEndpoint>.For(x => x.do_stuff(null), "stuff/:key", "GET");
            method.AddParameter("key");

            var value = "foo";
            var dict = method.ToParameters(x => x.do_stuff(value));
            dict.Count.ShouldBe(1);
            dict["key"].ShouldBe(value);
        }


        [Fact]
        public void resolve_properties_for_one_parameter_passed_as_constant()
        {
            var method = MethodRoute<FakeEndpoint>.For(x => x.do_stuff(null), "stuff/:key", "GET");
            method.AddParameter("key");

            var dict = method.ToParameters(x => x.do_stuff("foo"));
            dict.Count.ShouldBe(1);
            dict["key"].ShouldBe("foo");
        }

        [Fact]
        public void resolve_properties_for_one_parameter_passed_as_method_arg()
        {
            var method = MethodRoute<FakeEndpoint>.For(x => x.do_stuff(null), "stuff/:key", "GET");
            method.AddParameter("key");

            var dict = fetch(method, "bar");
            dict.Count.ShouldBe(1);
            dict["key"].ShouldBe("bar");
        }

        private IDictionary<string, string> fetch(MethodRoute<FakeEndpoint> method, string value)
        {
            return method.ToParameters(x => x.do_stuff(value));
        }


        [Fact]
        public void resolve_properties_for_multiple_parameters()
        {
            var method = MethodRoute<FakeEndpoint>.For(x => x.complex(null, 3), "stuff/:key/:number", "GET");
            method.AddParameter("key");
            method.AddParameter("number");

            var dict = method.ToParameters(x => x.complex("Mat", 5));
            dict["key"].ShouldBe("Mat");
            dict["number"].ShouldBe("5");
        }

        [Fact]
        public void register_itself()
        {
            var method = MethodRoute<FakeEndpoint>.For(x => x.do_stuff(null), "stuff/:key", "GET");
            var graph = Substitute.For<IUrlGraph>();

            method.Register(graph);

            graph.Received().Register(method.Leaf.Name, method);

            graph.Received().RegisterByHandler(typeof(FakeEndpoint), method.Method, method);
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