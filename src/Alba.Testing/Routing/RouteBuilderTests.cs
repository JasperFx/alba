using Alba.Routing;
using Shouldly;
using Xunit;

namespace Alba.Testing.Routing
{
    public class RouteBuilderTests
    {
        [Fact]
        public void use_get_as_the_verb_by_default()
        {
            var route = RouteBuilder.Build<SomeEndpoint>(x => x.Go());
            route.HttpMethod.ShouldBe(HttpVerbs.GET);
        }

        [Fact]
        public void assign_the_handler_type_and_method()
        {
            var route = RouteBuilder.Build<SomeEndpoint>(x => x.Go());
            route.HandlerType.ShouldBe(typeof(SomeEndpoint));
            route.Method.Name.ShouldBe("Go");
        }

        [Fact]
        public void assign_the_input_type_if_is_one()
        {
            var route = RouteBuilder.Build<SomeEndpoint>(x => x.post_something(null));
            route.InputType.ShouldBe(typeof(Input1));
        }

        [Fact]
        public void no_input_type_if_none()
        {
            var route = RouteBuilder.Build<SomeEndpoint>(x => x.delete_something(null));
            route.InputType.ShouldBeNull();
        }
    }

    public class SomeEndpoint
    {
        public void Go()
        {
            
        }

        public void post_something(Input1 input)
        {
            
        }

        public void delete_something(string name)
        {
            
        }
    }

    
}