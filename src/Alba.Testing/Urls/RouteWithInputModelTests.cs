using System;
using System.Collections.Generic;
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

        [Fact]
        public void write_a_string_field()
        {
            route.AddFieldParam("Key");

            var model = new InputModel();
            var dict = new Dictionary<string, string>();
            dict.Add("Key", "Thom");

            route.ApplyValues(model, dict);

            model.Key.ShouldBe("Thom");
        }

        [Fact]
        public void write_a_number_field()
        {
            route.AddFieldParam("Number");

            var model = new InputModel();
            var dict = new Dictionary<string, string>();
            dict.Add("Number", "11");

            route.ApplyValues(model, dict);

            model.Number.ShouldBe(11);
        }

        [Fact]
        public void write_an_enum_property()
        {
            route.AddPropertyParam("Color");

            var model = new InputModel();
            var dict = new Dictionary<string, string>();
            dict.Add("Color", "Blue");

            route.ApplyValues(model, dict);

            model.Color.ShouldBe(Color.Blue);

        }

        [Fact]
        public void mixed_field_and_property_write()
        {
            route.AddFieldParam("Number");
            route.AddPropertyParam("Color");

            var dict = new Dictionary<string, string>();
            dict.Add("Color", "Blue");
            dict.Add("Number", "11");


            var model = new InputModel();
            route.ApplyValues(model, dict);

            model.Number.ShouldBe(11);
            model.Color.ShouldBe(Color.Blue);
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