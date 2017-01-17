using System;
using System.Linq;
using Baseline;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class CookieParserTests
    {

        [Fact]
        public void one_simple_value()
        {
            var cookie = CookieParser.ToCookie("a=1");
            var state = cookie.States.Single();

            state.Name.ShouldBe("a");
            state.Value.ShouldBe("1");
        }

        [Fact]
        public void multiple_values()
        {
            var cookie = CookieParser.ToCookie("a=1; b=2");
            cookie.For("a").Value.ShouldBe("1");
            cookie.For("b").Value.ShouldBe("2");

        }

        [Fact]
        public void nested_values()
        {
            var cookie = CookieParser.ToCookie("a=1; b=b1=2&b2=3");
            cookie.For("a").Value.ShouldBe("1");

            var state = cookie.For("b");
            state["b1"].ShouldBe("2");
            state["b2"].ShouldBe("3");
        }

        [Fact]
        public void quoted_value_just_for_fun()
        {
            var cookie = CookieParser.ToCookie("a=\"some text\"");
            var state = cookie.States.Single();
            state.Value.ShouldBe("some text");
        }

        [Fact]
        public void sets_the_domain()
        {
            var cookie = CookieParser.ToCookie("a=1; domain=http://cnn.com");

            cookie.Domain.ShouldBe("http://cnn.com");
        }

        [Fact]
        public void sets_the_domain_is_case_insensitive()
        {
            var cookie = CookieParser.ToCookie("a=1; Domain=http://cnn.com");

            cookie.Domain.ShouldBe("http://cnn.com");
        }

        [Fact]
        public void set_The_max_age()
        {
            var cookie = CookieParser.ToCookie("a=1; domain=http://cnn.com; max-age=5");
            cookie.MaxAge.ShouldBe(5.Seconds());
        }

        [Fact]
        public void set_the_path()
        {
            var cookie = CookieParser.ToCookie("a=1; Domain=http://cnn.com; path=foo;");

            cookie.Path.ShouldBe("foo");
        }

        [Fact]
        public void set_The_path_with_default_value()
        {
            var cookie = CookieParser.ToCookie("a=1; Domain=http://cnn.com; path=;");

            cookie.Path.ShouldBe("/");
        }

        [Fact]
        public void set_The_path_with_default_value_2()
        {
            var cookie = CookieParser.ToCookie("a=1; Domain=http://cnn.com; path;");

            cookie.Path.ShouldBe("/");
        }

        [Fact]
        public void set_secure()
        {
            var cookie = CookieParser.ToCookie("a=1; Domain=http://cnn.com; secure;");
            cookie.Secure.ShouldBeTrue();
        }

        [Fact]
        public void set_secure_is_case_insensitive()
        {
            var cookie = CookieParser.ToCookie("a=1; Domain=http://cnn.com; Secure;");
            cookie.Secure.ShouldBeTrue();
        }

        [Fact]
        public void do_not_set_secure()
        {
            var cookie = CookieParser.ToCookie("a=1; Domain=http://cnn.com;");
            cookie.Secure.ShouldBeFalse();
        }

        [Fact]
        public void set_http_only()
        {
            var cookie = CookieParser.ToCookie("a=1; Domain=http://cnn.com; httponly;");

            cookie.HttpOnly.ShouldBeTrue();
        }

        [Fact]
        public void set_http_only_is_case_insensitive()
        {
            var cookie = CookieParser.ToCookie("a=1; Domain=http://cnn.com; HttpOnly;");

            cookie.HttpOnly.ShouldBeTrue();
        }

        [Fact]
        public void http_only_is_not_set_by_default()
        {
            var cookie = CookieParser.ToCookie("a=1; Domain=http://cnn.com;");
            cookie.HttpOnly.ShouldBeFalse();
        }

        [Fact]
        public void expires_is_not_set_by_default()
        {
            var cookie = CookieParser.ToCookie("a=1; Domain=http://cnn.com;");
            cookie.Expires.ShouldBeNull();
        }

        [Fact]
        public void expires_smoke()
        {
            var date = DateTime.Today.AddHours(5).ToUniversalTime();

            var cookie = CookieParser.ToCookie("a=1; Domain=http://cnn.com; expires=" + date.ToString("dddd, d'-'MMM'-'yy H:m:s 'GMT'"));

            cookie.Expires.Value.ToLocalTime().TimeOfDay.ShouldBe(date.ToLocalTime().TimeOfDay);
        }
    }
}