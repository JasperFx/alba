using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;

namespace AlbaRouter.Testing
{
    public class Route_resolving_urls_with_input_model_Tests
    {
        private readonly RequestDelegate Empty = c => Task.CompletedTask;
            
            
            
        [Fact]
        public void get_parameters_from_field()
        {
            var route = new Route("foo/:Key", "GET", Empty);
            route.GetArgument("Key").MapToField<InputModel>("Key");

            var dict = route.ToParameters(new InputModel {Key = "Rand"});

            dict.Count.ShouldBe(1);
            dict["Key"].ShouldBe("Rand");
        }

        [Fact]
        public void get_parameters_from_property()
        {
            var route = new Route("foo/:Color", "GET", Empty);
            route.GetArgument("Color").MapToProperty<InputModel>(x => x.Color);

            var dict = route.ToParameters(new InputModel {Color = Color.Blue});

            dict["Color"].ShouldBe("Blue");
        }

        [Fact]
        public void multiple_field_and_property()
        {
            var route = new Route("foo/:Color/:Key", "GET", Empty);
            route.GetArgument("Color").MapToProperty<InputModel>(x => x.Color);
            route.GetArgument("Key").MapToField<InputModel>("Key");

            var dict = route.ToParameters(new InputModel { Color = Color.Blue, Key = "Perrin"});

            dict["Color"].ShouldBe("Blue");
            dict["Key"] = "Perrin";
        }


        [Fact]
        public void write_a_string_field()
        {
            var route = new Route("foo/:Key", "GET", Empty);
            route.InputType = typeof (InputModel);
            route.GetArgument("Key").MapToField<InputModel>("Key");

            var model = new InputModel();
            var dict = new Dictionary<string, object>();
            dict.Add("Key", "Thom");

            route.WriteToInputModel(model, dict);

            model.Key.ShouldBe("Thom");
        }

        [Fact]
        public void write_a_number_field()
        {
            var route = new Route("foo/:Number", "GET", Empty);
            route.InputType = typeof(InputModel);
            route.GetArgument("Number").MapToField<InputModel>("Number");

            var model = new InputModel();
            var dict = new Dictionary<string, object>();
            dict.Add("Number", 11);

            route.WriteToInputModel(model, dict);

            model.Number.ShouldBe(11);
        }

        [Fact]
        public void write_an_enum_property()
        {
            var route = new Route("foo/:Color", "GET", Empty);
            route.InputType = typeof(InputModel);
            route.GetArgument("Color").MapToProperty<InputModel>(x => x.Color);


            var model = new InputModel();
            var dict = new Dictionary<string, object>();
            dict.Add("Color", Color.Blue);

            route.WriteToInputModel(model, dict);

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