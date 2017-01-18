using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Alba.Testing.Acceptance
{
    public class assertions_against_response_headers : ScenarioContext
    {
        [Fact]
        public Task single_header_value_is_positive()
        {
            return host.Scenario(x =>
            {
                x.Post.Json(new HeaderInput { Key = "Foo", Value1 = "Bar" }).Accepts("text/plain");

                x.Header("Foo").ShouldHaveOneNonNullValue()
                    .SingleValueShouldEqual("Bar");
            });
        }

        [Fact]
        public async Task single_header_value_is_negative_with_the_wrong_value()
        {
            var ex = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Post.Json(new HeaderInput { Key = "Foo", Value1 = "NotBar" });
                    x.Header("Foo").ShouldHaveOneNonNullValue()
                        .SingleValueShouldEqual("Bar");
                });
            });

            ex.Message.ShouldContain("Expected a single header value of 'Foo'='Bar', but the actual value was 'NotBar'");
        }

        [Fact]
        public async Task single_header_value_is_negative_with_the_too_many_values()
        {
            var ex = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Post.Json(new HeaderInput { Key = "Foo", Value1 = "NotBar", Value2 = "AnotherBar" });
                    x.Header("Foo").ShouldHaveOneNonNullValue()
                        .SingleValueShouldEqual("Bar");
                });
            });

            ex.Message.ShouldContain(
                "Expected a single header value of 'Foo'='Bar', but the actual values were 'NotBar', 'AnotherBar'");
        }

        [Fact]
        public Task header_should_not_be_written_happy_path()
        {
            return host.Scenario(x =>
            {
                x.Post.Json(new HeaderInput { Key = "Foo" }).Accepts("text/plain");
                x.Header("Foo").ShouldNotBeWritten();
            });
        }

        [Fact]
        public async Task header_should_not_be_written_sad_path_with_values()
        {
            var ex = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Post.Json(new HeaderInput { Key = "Foo", Value1 = "Bar1", Value2 = "Bar2" }).Accepts("plain/text");
                    x.Header("Foo").ShouldNotBeWritten();
                });
            });

            ex.Message.ShouldContain("Expected no value for header 'Foo', but found values 'Bar1', 'Bar2'");
        }


        [Fact]
        public async Task should_have_on_non_null_value_sad_path_with_too_many_values()
        {
            var ex = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Post.Json(new HeaderInput { Key = "Foo", Value1 = "Bar1", Value2 = "Bar2" });
                    x.Header("Foo").ShouldHaveOneNonNullValue();
                });
            });

            ex.Message.ShouldContain(
                "Expected a single header value of 'Foo', but found multiple values on the response: 'Bar1', 'Bar2'");
        }


        [Fact]
        public Task should_have_on_non_null_header_value_happy_path()
        {
            return host.Scenario(x =>
            {
                x.Post.Json(new HeaderInput { Key = "Foo", Value1 = "Anything" }).Accepts("text/plain");
                x.Header("Foo").ShouldHaveOneNonNullValue();
            });
        }

        [Fact]
        public async Task should_have_one_non_null_value_sad_path()
        {
            var ex = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Post.Json(new HeaderInput { Key = "Foo" }).Accepts("text/plain");
                    x.Header("Foo").ShouldHaveOneNonNullValue();
                });
            });

            ex.Message.ShouldContain("Expected a single header value of 'Foo', but no values were found on the response");
        }


        [Fact]
        public async Task single_header_value_is_negative_because_there_are_no_values()
        {
            var ex = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Post.Json(new HeaderInput { Key = "Foo" });
                    x.Header("Foo")
                        .SingleValueShouldEqual("Bar");
                });
            });

            ex.Message.ShouldContain(
                "Expected a single header value of 'Foo'='Bar', but no values were found on the response");
        }


    }
}