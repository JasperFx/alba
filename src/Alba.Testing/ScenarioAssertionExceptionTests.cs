using Microsoft.AspNetCore.Http;
using Shouldly;

namespace Alba.Testing
{
    public class ScenarioAssertionExceptionTests
    {
        [Fact]
        public void assert_does_nothing_with_no_messages()
        {
            var ex = new ScenarioAssertionException();

            ex.AssertAll(); // all good!
        }

        [Fact]
        public void assert_with_any_messages_blows_up()
        {
            var ex = new ScenarioAssertionException();
            ex.Add("You stink!");

            Exception<ScenarioAssertionException>.ShouldBeThrownBy(() => ex.AssertAll());
        }

        [Fact]
        public void all_messages_are_in_the_ex_message()
        {
            var ex = new ScenarioAssertionException();
            ex.Add("You stink!");
            ex.Add("You missed a header!");

            ex.Message.ShouldContain("You stink!");
            ex.Message.ShouldContain("You missed a header!");
        }

        [Fact]
        public void show_the_body_in_the_message_if_set()
        {
            var ex = new ScenarioAssertionException();
            var ctx = new DefaultHttpContext();
            ctx.Response.Body = new MemoryStream();
            var body = "<html></html>";
            using var sw = new StreamWriter(ctx.Response.Body);
            sw.Write(body);
            sw.Flush();
            
            var context = new AssertionContext(ctx, ex);
            ex.Add("You stink!");

            ex.Message.ShouldNotContain("Actual body text was:");

            context.ReadBodyAsString();
            ex.Message.ShouldContain("Actual body text was:");
            ex.Message.ShouldContain(body);
        }
    }
}