using Shouldly;
using Xunit;

namespace AlbaRouter.Testing
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
        public void picks_up_custom_route_name_from_attribute_if_any()
        {
            var route = RouteBuilder.Build<SomeEndpoint>(x => x.Named());
            route.Name.ShouldBe("Finn");
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

        [RouteName("Finn")]
        public void Named()
        {
            throw new System.NotImplementedException();
        }
    }

    
}