using Microsoft.AspNetCore.Http;
using Shouldly;

namespace Alba.Testing.Acceptance
{
    public class assertions_against_response_headers : ScenarioContext
    {
        [Fact]
        public Task single_header_value_is_positive()
        {
            router.Handlers["/one"] = c => {
                c.Response.Headers.Append("Foo", "Bar");

                return Task.CompletedTask;
            };

            return host.Scenario(x =>
            {
                x.Post.Url("/one");

                x.Header("Foo").ShouldHaveOneNonNullValue()
                    .SingleValueShouldEqual("Bar");
            });
        }

        [Fact]
        public async Task single_header_value_is_negative_with_the_wrong_value()
        {
            router.Handlers["/one"] = c => {
                c.Response.Headers.Append("Foo", "NotBar");

                return Task.CompletedTask;
            };

            var ex = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Post.Url("/one");
                    x.Header("Foo").ShouldHaveOneNonNullValue()
                        .SingleValueShouldEqual("Bar");
                });
            });

            ex.Message.ShouldContain("Expected a single header value of 'Foo'='Bar', but the actual value was 'NotBar'");
        }

        [Fact]
        public async Task single_header_value_is_negative_with_the_too_many_values()
        {
            router.Handlers["/one"] = c => {
                c.Response.Headers.Append("Foo", "NotBar");
                c.Response.Headers.Append("Foo", "AnotherBar");

                return Task.CompletedTask;
            };

            var ex = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Post.Url("/one");
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
            router.Handlers["/one"] = c => Task.CompletedTask;

            return host.Scenario(x =>
            {
                x.Post.Url("/one");
                x.Header("Foo").ShouldNotBeWritten();
            });
        }

        [Fact]
        public async Task header_should_not_be_written_sad_path_with_values()
        {
            router.Handlers["/one"] = c => {
                c.Response.Headers.Append("Foo", "Bar1");
                c.Response.Headers.Append("Foo", "Bar2");

                return Task.CompletedTask;
            };

            var ex = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Post.Url("/one");
                    x.Header("Foo").ShouldNotBeWritten();
                });
            });

            ex.Message.ShouldContain("Expected no value for header 'Foo', but found values 'Bar1', 'Bar2'");
        }

        [Fact]
        public async Task should_have_on_non_null_value_sad_path_with_too_many_values()
        {
            router.Handlers["/one"] = c =>
            {
                c.Response.Headers.Append("Foo", "Bar1");
                c.Response.Headers.Append("Foo", "Bar2");

                return Task.CompletedTask;
            };

            var ex = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Post.Url("/one");
                    x.Header("Foo").ShouldHaveOneNonNullValue();
                });
            });

            ex.Message.ShouldContain(
                "Expected a single header value of 'Foo', but found multiple values on the response: 'Bar1', 'Bar2'");
        }

        [Fact]
        public Task should_have_on_non_null_header_value_happy_path()
        {
            router.Handlers["/one"] = c =>
            {
                c.Response.Headers.Append("Foo", "Anything");

                return Task.CompletedTask;
            };

            return host.Scenario(x =>
            {
                x.Post.Url("/one");
                x.Header("Foo").ShouldHaveOneNonNullValue();
            });
        }

        [Fact]
        public async Task should_have_one_non_null_value_sad_path()
        {
            router.Handlers["/one"] = c =>
            {
                return Task.CompletedTask;
            };

            var ex = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Post.Url("/one");
                    x.Header("Foo").ShouldHaveOneNonNullValue();
                });
            });

            ex.Message.ShouldContain("Expected a single header value of 'Foo', but no values were found on the response");
        }

        [Fact]
        public async Task single_header_value_is_negative_because_there_are_no_values()
        {
            router.Handlers["/one"] = c =>
            {
                return Task.CompletedTask;
            };

            var ex = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Post.Url("/one");
                    x.Header("Foo")
                        .SingleValueShouldEqual("Bar");
                });
            });

            ex.Message.ShouldContain(
                "Expected a single header value of 'Foo'='Bar', but no values were found on the response");
        }

        [Fact]
        public Task muliple_header_values_happy_path()
        {
            router.Handlers["/one"] = c =>
            {
                c.Response.Headers.Append("Foo", "Anything");
                c.Response.Headers.Append("Foo", "Bar");
                return Task.CompletedTask;
            };

            return host.Scenario(x =>
            {
                x.Post.Url("/one");
                x.Header("Foo").ShouldHaveValues("Anything", "Bar");
            });
        }

        [Fact]
        public async Task multiple_header_values_wrong_value()
        {
            router.Handlers["/one"] = c =>
            {
                c.Response.Headers.Append("Foo", "Anything");
                return Task.CompletedTask;
            };

            var ex = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Post.Url("/one");
                    x.Header("Foo").ShouldHaveValues("Bar");
                });
            });

            ex.Message.ShouldContain(
                "Expected header values of 'Foo'='Bar', but the actual values were 'Anything'.");
        }

        [Fact]
        public async Task multiple_header_values_no_values()
        {
            router.Handlers["/one"] = c => Task.CompletedTask;

            var ex = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Post.Url("/one");
                    x.Header("Foo").ShouldHaveValues("Bar");
                });
            });

            ex.Message.ShouldContain(
                "Expected header values of 'Foo'='Bar', but no values were found on the response.");
        }

        [Fact]
        public async Task multiple_header_values_bad_argument()
        {
            router.Handlers["/one"] = c => Task.CompletedTask;

            var ex = await Exception<ArgumentException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Post.Url("/one");
                    x.Header("Foo").ShouldHaveValues(Array.Empty<string>());
                });
            });

            ex.Message.ShouldContain(
                "Expected values must contain at least one value");
        }

        [Fact]
        public Task header_values_exist_happy_path()
        {
            router.Handlers["/one"] = c =>
            {
                c.Response.Headers.Append("Foo", "Anything");
                c.Response.Headers.Append("Foo", "Bar");
                return Task.CompletedTask;
            };

            return host.Scenario(x =>
            {
                x.Post.Url("/one");
                x.Header("Foo").ShouldHaveValues();
            });
        }

        [Fact]
        public async Task header_values_exist_empty_value()
        {
            router.Handlers["/one"] = c =>
            {
                c.Response.Headers.Append("Foo", "");
                return Task.CompletedTask;
            };

            var ex = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Post.Url("/one");
                    x.Header("Foo").ShouldHaveValues();
                });
            });

            ex.Message.ShouldContain(
                "Expected header 'Foo' to be present but no values were found on the response."); ;
        }


        [Fact]
        public async Task header_values_exist_no_values()
        {
            router.Handlers["/one"] = c => Task.CompletedTask;

            var ex = await Exception<ScenarioAssertionException>.ShouldBeThrownBy(() =>
            {
                return host.Scenario(x =>
                {
                    x.Post.Url("/one");
                    x.Header("Foo").ShouldHaveValues();
                });
            });

            ex.Message.ShouldContain(
                "Expected header 'Foo' to be present but no values were found on the response.");
        }



    }
}