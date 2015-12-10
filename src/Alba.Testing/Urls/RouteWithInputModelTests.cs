using System;
using Alba.Routing;
using Alba.Urls;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Alba.Testing.Urls
{
    public class RouteWithInputModelTests
    {
        private readonly RouteWithInputModel<InputModel> route 
            = new RouteWithInputModel<InputModel>(Leaf.For("/something"), "GET");

        [Fact]
        public void has_parameters_with_none()
        {
            route.HasParameters.ShouldBeFalse();
        }

        [Fact]
        public void has_parameters_with_any()
        {
            route.AddFieldParam("Key");
            route.HasParameters.ShouldBeTrue();
        }


        [Fact]
        public void get_parameters_from_field()
        {
            route.AddFieldParam("Key");

            var dict = route.ToParameters(new InputModel {Key = "Rand"});

            dict.Count.ShouldBe(1);
            dict["Key"].ShouldBe("Rand");
        }

        [Fact]
        public void get_parameters_from_property()
        {
            route.AddPropertyParam("Color");

            var dict = route.ToParameters(new InputModel {Color = Color.Blue});

            dict["Color"].ShouldBe("Blue");
        }

        [Fact]
        public void multiple_field_and_property()
        {
            route.AddFieldParam("Key");
            route.AddPropertyParam("Color");

            var dict = route.ToParameters(new InputModel { Color = Color.Blue, Key = "Perrin"});

            dict["Color"].ShouldBe("Blue");
            dict["Key"] = "Perrin";
        }

        [Fact]
        public void registers_itself()
        {
            var graph = Substitute.For<IUrlGraph>();

            route.Register(graph);

            graph.Received().Register(route.Leaf.Name, route);
            graph.Received().RegisterByInput(typeof(InputModel), route);
        }
    }

    public class InputModel
    {
        public string Key;
        public int Number;
        public double Limit { get; set; }
        public DateTime Expiration { get; set; }

        public Color Color { get; set; }
    }

    public enum Color
    {
        Red,
        Blue,
        Yellow
    }
}