using System;
using System.Globalization;
using System.Linq;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class CookieTests
    {
        [Fact]
        public void matches()
        {
            var cookie = new Cookie();
            cookie.Matches("a").ShouldBeFalse();

            cookie.Add(new CookieState("something"));

            cookie.Matches("a").ShouldBeFalse();
            cookie.Matches("something").ShouldBeTrue();

            cookie.Add(new CookieState("Else"));
            cookie.Matches("something").ShouldBeTrue();
            cookie.Matches("else").ShouldBeTrue();
            cookie.Matches("a").ShouldBeFalse();
        }

        [Fact]
        public void to_string_smoke_testing()
        {
            roundTrip("a=1; b=2");
            roundTrip("a=1; b=b1=1&b2=2");
            roundTrip("a=1; b=b1=1&b2=2; httponly");
            roundTrip("a=1; b=b1=1&b2=2; secure; httponly");
            roundTrip("a=1; b=b1=1&b2=2; secure");
            roundTrip("a=1; domain=http://cnn.com");
            roundTrip("a=1; expires=Fri, 08 Aug 2014 22:30:58 GMT; path=/");
            roundTrip("a=1; max-age=5; domain=http://cnn.com");
            roundTrip("a=1; domain=http://cnn.com; path=foo");
            roundTrip("a=1; max-age=5; domain=http://cnn.com; path=foo; secure; httponly");
        }

        [Fact]
        public void to_string_expires()
        {
            var cookie = new Cookie("something");
            cookie.Expires = DateTime.Today.AddHours(10);

            cookie.ToString().ShouldContain("expires=" + cookie.Expires.Value.ToString("r", CultureInfo.InvariantCulture));

            cookie.Path = "/";
            cookie.ToString().ShouldContain("expires=" + cookie.Expires.Value.ToString("r", CultureInfo.InvariantCulture) + ";");
        }

        [Fact]
        public void value_with_only_one_state_with_only_one_value()
        {
            var cookie = new Cookie("a", "2");
            cookie.Value.ShouldBe("2");
        }

        [Fact]
        public void can_set_value_for_single_value_cookies()
        {
            var cookie = new Cookie("a", "2");
            cookie.Value.ShouldBe("2");

            cookie.Value = "3";

            cookie.Value.ShouldBe("3");

            cookie.States.Single().Value.ShouldBe("3");
        }

        [Fact]
        public void value_with_more_than_one_state()
        {
            var cookie = new Cookie("a").Add(new CookieState("a1", "1"));
            cookie.Value.ShouldBeNull();

        }

        [Fact]
        public void value_with_no_states()
        {
            var cookie = new Cookie("foo");
            cookie.Value.ShouldBeNull();
        }



        private void roundTrip(string text)
        {
            var cookie = CookieParser.ToCookie(text);
            cookie.ToString().ShouldBe(text);
        }

        [Fact]
        public void get_value()
        {
            var cookie = new Cookie("foo").Add(new CookieState("a", "1")).Add(new CookieState("b", "2"));

            cookie.GetValue("a").ShouldBe("1");
            cookie.GetValue("b").ShouldBe("2");
        }

        [Fact]
        public void split_values()
        {
            CookieParser.SplitValues("a=b;c=d;").ShouldHaveTheSameElementsAs("a=b", "c=d");
        }
    }
}