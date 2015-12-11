using System;
using System.Collections.Generic;
using Alba.Routing;
using Shouldly;
using Xunit;

namespace Alba.Testing.Routing
{
    public class RouteArgumentTests
    {
        [Fact]
        public void happy_path()
        {
            var env = new Dictionary<string, object>();

            var parameter = new RouteArgument("foo", 1);

            parameter.SetValues(env, "a/b/c/d".Split('/'));

            env.GetRouteData("foo").ShouldBe("b");
        }

        [Fact]
        public void happy_path_with_number()
        {
            var env = new Dictionary<string, object>();

            var parameter = new RouteArgument("foo", 1);
            parameter.ArgType = typeof (int);

            parameter.SetValues(env, "a/25/c/d".Split('/'));

            env.GetRouteData("foo").ShouldBe(25);
        }

        [Fact]
        public void canonical_path()
        {
            var parameter = new RouteArgument("foo", 1);
            parameter.CanonicalPath().ShouldBe("*");
        }

        [Fact]
        public void the_default_arg_type_is_string()
        {
            var parameter = new RouteArgument("foo", 1);
            parameter.ArgType.ShouldBe(typeof(string));
        }

        [Fact]
        public void can_override_the_parameter_arg_type()
        {
            var parameter = new RouteArgument("foo", 1);
            parameter.ArgType = typeof (int);

            parameter.ArgType.ShouldBe(typeof(int));
        }


        [Fact]
        public void get_parameters_from_field()
        {
            var arg = new RouteArgument("Key", 0);
            arg.MapToField<InputModel>("Key");

            arg.ArgType.ShouldBe(typeof(string));

            arg.ReadRouteDataFromInput(new InputModel {Key = "Rand"})
                .ShouldBe("Rand");

            
        }

        [Fact]
        public void get_parameters_from_number_field()
        {
            var arg = new RouteArgument("Key", 0);
            arg.MapToField<InputModel>("Number");

            arg.ArgType.ShouldBe(typeof(int));

            arg.ReadRouteDataFromInput(new InputModel { Number = 25})
                .ShouldBe("25");


        }

        [Fact]
        public void write_value_to_field()
        {
            var arg = new RouteArgument("Key", 0);
            arg.MapToField<InputModel>("Key");

            var model = new InputModel();
            var dict = new Dictionary<string, object>();
            dict.Add(arg.Key, "Mat");

            arg.ApplyRouteDataToInput(model, dict);

            model.Key.ShouldBe("Mat");
        }

        [Fact]
        public void write_value_to_property()
        {
            var arg = new RouteArgument("Color", 2);
            arg.MapToProperty<InputModel>(x => x.Color);

            var model = new InputModel();
            var dict = new Dictionary<string, object>();
            dict.Add("Color", Color.Yellow);

            arg.ApplyRouteDataToInput(model, dict);

            model.Color.ShouldBe(Color.Yellow);

        }

        [Fact]
        public void get_parameters_from_property()
        {
            var arg = new RouteArgument("Key", 0);
            arg.MapToProperty<InputModel>(x => x.Color);


            arg.ReadRouteDataFromInput(new InputModel {Color = Color.Blue})
                .ShouldBe("Blue");
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
}