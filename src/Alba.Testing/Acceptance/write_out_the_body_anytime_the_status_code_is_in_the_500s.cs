using Microsoft.AspNetCore.Http;
using Shouldly;

namespace Alba.Testing.Acceptance
{
    public class write_out_the_body_anytime_the_status_code_is_in_the_500s : ScenarioContext
    {
        [Fact]
        public async Task will_write_out_the_body_on_500()
        {
            router.Handlers["/one"] = c =>
            {
                c.Response.StatusCode = 500;
                return c.Response.WriteAsync(new NotSupportedException().ToString());
            };

            var ex = await fails(_ =>
            {
                _.Get.Url("/one");
                _.StatusCodeShouldBeOk();
            });

            ex.Message.ShouldContain("NotSupportedException");
        }

        [Fact]
        public async Task will_write_out_the_body_on_501()
        {
            router.Handlers["/one"] = c =>
            {
                c.Response.StatusCode = 501;
                return c.Response.WriteAsync(new NotSupportedException().ToString());
            };

            var ex = await fails(_ =>
            {
                _.Get.Url("/one");
                _.StatusCodeShouldBeOk();
            });

            ex.Message.ShouldContain("NotSupportedException");
        }

        [Fact]
        public async Task will_write_out_the_body_on_502()
        {
            router.Handlers["/one"] = c =>
            {
                c.Response.StatusCode = 501;
                return c.Response.WriteAsync(new DivideByZeroException().ToString());
            };

            var ex = await fails(_ =>
            {
                _.Get.Url("/one");
                _.StatusCodeShouldBeOk();
            });

            ex.Message.ShouldContain("DivideByZeroException");
        }
    }
}