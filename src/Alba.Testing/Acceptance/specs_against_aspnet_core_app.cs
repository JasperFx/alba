using System.Net.Http.Headers;
using System.Net.Mime;
using Shouldly;
using WebApp;
using WebApp.Controllers;

namespace Alba.Testing.Acceptance
{
    public class specs_against_aspnet_core_app : IAsyncLifetime
    {
        private IAlbaHost _system;

        private Task<IScenarioResult> run(Action<Scenario> configuration)
        {
            return _system.Scenario(configuration);
        }
        

        [Fact]
        public void services_are_non_null()
        {
            _system.Services.ShouldNotBeNull();
        }

        [Fact]
        public Task bootstrap_and_execute_a_request_through_an_aspnet_core_app()
        {
            return run(_ =>
            {
                _.Get.Url("/api/values").Accepts("text/plain");
                _.ContentTypeShouldBe("text/plain; charset=utf-8");
                _.StatusCodeShouldBeOk();
            });
        }

        [Fact]
        public async Task obeys_the_http_method()
        {
            await run(_ =>
            {
                _.Post.Url("/api/values");
                _.Body.TextIs("Blue");

                _.ContentShouldBe("I ran a POST with value Blue");
            });

            await run(_ =>
            {
                _.Patch.Url("/api/values");
                _.Body.TextIs("Blue");

                _.ContentShouldBe("I ran a PATCH with value Blue");
            });
        }

        [Fact]
        public Task can_do_the_parallel_directory_trick_from_fubu_for_content_data()
        {
            return run(_ =>
            {
                _.Get.Url("/hello.txt");
                _.ContentShouldContain("Hello from ASP.Net Core app!");
            });
        }

        [Fact]
        public async Task can_read_json_response()
        {
            var result = await run(_ =>
            {
                _.Get.Url("/api/json");
            });


            var person = result.ReadAsJson<Person>();


            person.FirstName.ShouldBe("Jeremy");
            person.LastName.ShouldBe("Miller");
        }

        [Fact]
        public async Task can_read_json_response_async()
        {
            var result = await run(_ =>
            {
                _.Get.Url("/api/json");
            });

            var person = await result.ReadAsJsonAsync<Person>();

            person.FirstName.ShouldBe("Jeremy");
            person.LastName.ShouldBe("Miller");
        }

        [Fact]
        public async Task repeated_reads_of_the_response_async()
        {
            var expectedJson = "{\"firstName\":\"Jeremy\",\"lastName\":\"Miller\"}";
            var result = await run(_ =>
            {
                _.Get.Url("/api/json");
                _.Body.TextIs(expectedJson);
            });

            var text = await result.ReadAsTextAsync();
            text.ShouldBe(expectedJson);

            var person = await result.ReadAsJsonAsync<Person>();

            person.FirstName.ShouldBe("Jeremy");
            person.LastName.ShouldBe("Miller");
        }

        [Fact]
        public async Task Bug_92_repeated_reads_of_the_response()
        {
            var expectedJson = "{\"firstName\":\"Jeremy\",\"lastName\":\"Miller\"}";
            var result = await run(_ =>
            {
                _.Get.Url("/api/json");
                _.Body.TextIs(expectedJson);
            });
            
            result.ReadAsText().ShouldBe(expectedJson);

            var person = result.ReadAsJson<Person>();

            person.FirstName.ShouldBe("Jeremy");
            person.LastName.ShouldBe("Miller");
        }


        [Fact]
        public async Task Bug_92_repeated_reads_of_the_response_2()
        {
            var expectedJson = "{\"firstName\":\"Jeremy\",\"lastName\":\"Miller\"}";
            var result = await run(_ =>
            {
                _.Get.Url("/api/json");
                _.Body.TextIs(expectedJson);
            });

            var person = result.ReadAsJson<Person>();
            
            result.ReadAsText().ShouldBe(expectedJson);

            person.FirstName.ShouldBe("Jeremy");
            person.LastName.ShouldBe("Miller");
        }

        [Fact]
        public async Task useful_exception_message_on_bad_json()
        {
            var result = await run(_ =>
            {
                _.Get.Url("/api/values");
            });

            var ex = Exception<AlbaJsonFormatterException>.ShouldBeThrownBy(() =>
            {
                var answer = result.ReadAsJson<Person>();
            });
            
            ex.Message.ShouldContain("The JSON formatter was unable to process the raw JSON");
            ex.Message.ShouldContain("value1, value2");
        }
        
        [Fact]
        public async Task useful_exception_message_on_empty()
        {
            var result = await run(_ =>
            {
                _.Get.Url("/api/values");
            });

            var ex = Exception<AlbaJsonFormatterException>.ShouldBeThrownBy(() =>
            {
                var answer = result.ReadAsJson<Person>();
            });
            
            ex.Message.ShouldContain("The JSON formatter was unable to process the raw JSON");
            ex.Message.ShouldContain("value1, value2");
        }

        [Fact]
        public async Task useful_exception_when_response_body_is_empty()
        {
            var result = await run(_ =>
            {
                _.Get.Url("/empty");
            });

            var ex = Exception<EmptyResponseException>.ShouldBeThrownBy(() =>
            {
                var answer = result.ReadAsJson<Person>();
            });

        }


        [Fact]
        public async Task can_post_json_response()
        {

            var result = await run(_ =>
            {
                _.Post
                    .Json(new Person {FirstName = "Tom", LastName = "Brady"})
                    .ToUrl("/api/json");

            });

            var person = result.ReadAsJson<Person>();

            person.FirstName.ShouldBe("Tom");
            person.LastName.ShouldBe("Brady");
        }


        [Fact]
        public Task can_send_querystring_parameters()
        {
            return run(_ =>
            {
                _.Get.Url("/querystring2").QueryString("test", "value");
                _.ContentShouldContain("test=value");
            });
        }

        [Fact]
        public Task returns_successfully_when_passed_input_is_passes_to_URL_query_string()
        {
            return run(_ =>
            {
                _.Get.Url("/querystring2?test=value");
                _.ContentShouldContain("test=value");
            });
        }

        [Fact]
        public Task returns_succesfully_with_array_values_by_index()
        {
            return run(_ =>
            {
                _.Get.Url("/querystringarray?tests[0]=value1&tests[1]=value2");
                _.ContentShouldContain("value1");
                _.ContentShouldContain("value2");
            });
        }

        [Fact]
        public Task returns_successfully_with_array_and_duplicate_keys()
        {
            return run(_ =>
            {
                _.Get.Url("/querystringarray?tests=value1&tests=value2");
                _.ContentShouldContain("value1");
                _.ContentShouldContain("value2");
            });
        }

        [Fact]
        public Task query_string_with_multiple_values()
        {
            return run(_ =>
            {
                _.Get.Url("/querystring2?test=value&foo=bar");
                _.ContentShouldContain("test=value");
                _.ContentShouldContain("foo=bar");
            });
        }

        [Fact]
        public Task query_string_with_multiple_values_by_target()
        {
            return run(_ =>
            {
                _.Get.Url("/querystring2").QueryString(new {test = "value", foo = "bar"});
                _.ContentShouldContain("test=value");
                _.ContentShouldContain("foo=bar");
            });
        }

        [Fact]
        public Task query_string_with_multiple_values_by_target_with_strong_typed_argument()
        {
            return run(_ =>
            {
                _.Get.Url("/querystring2").QueryString(new QueryStringTarget{ test = "value", foo = "bar" });
                _.ContentShouldContain("test=value");
                _.ContentShouldContain("foo=bar");
            });
        }

        public class QueryStringTarget
        {
            public string test;
            public string foo { get; set; }
        }

        [Fact]
        public Task send_form_to_aspnetcore_with_FromForm()
        {
            return run(_ =>
            {
                _.Post.Url("/sendform");
                _.Body.WriteFormData(new Dictionary<string, string>{{"test", "foo"}});

                _.ContentShouldContain("foo");
            });
        }

        [Fact]
        public Task send_body_to_aspnetcore_with_FromBody()
        {
            return run(_ =>
            {
                _.Post.Url("/sendbody");
                _.Body.TextIs("some stuff?");

                _.ContentShouldContain("some stuff?");
            });
        }

        [Fact]
        public async Task send_file_to_aspnetcore_with_MultipartForm()
        {
            // text file
            await using var textFile = File.OpenRead("TestTextFile.txt");
            var textFileName = Path.GetFileName(textFile.Name);
            using var content = new StreamContent(textFile);
            content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Text.Plain);

            // image
            await using var imageFile = File.OpenRead("TestImage.jpg");
            var imageFileName = Path.GetFileName(imageFile.Name);
            using var content2 = new StreamContent(imageFile);
            content2.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Image.Jpeg);

            using var formData = new MultipartFormDataContent();

            formData.Add(content, "files", textFileName);
            formData.Add(content2, "files", imageFileName);

            // another random form value
            var additional = "My additional string of fun times";
            var additionalContent = new StringContent(additional);
            formData.Add(additionalContent, "additionalContent");

            var result = await run(_ =>
            {
                _.Post.MultipartFormData(formData).ToUrl("/api/files/upload");
                _.StatusCodeShouldBeOk();
            });

            var json = await result.ReadAsJsonAsync<List<FilesController.UploadResponse>>();

            Assert.Contains(new FilesController.UploadResponse(textFileName, textFile.Length, "Hello there!", additional), json);
            Assert.Contains(new FilesController.UploadResponse(imageFileName, imageFile.Length, "image", additional), json);
        }

        [Fact]
        public Task returns_successfully_when_passed_object_is_passed_to_Input()
        {
            return run(_ =>
            {
                _.Get.Url("/querystring?test=somevalue");

                _.ContentShouldContain("somevalue");
            });
        }

        public async ValueTask InitializeAsync()
        {
            _system = await AlbaHost.For<Startup>();
        }

        public async ValueTask DisposeAsync()
        {
            await _system.DisposeAsync();
        }
    }
}