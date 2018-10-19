using System;
using System.Net;
using System.Threading.Tasks;
using Baseline;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Shouldly;
using WebApp;
using Xunit;

namespace Alba.Testing.Acceptance
{
    public class customize_json_serialization 
    {
        [Fact]
        public void all_defaults()
        {
            using (var host = SystemUnderTest.ForStartup<Startup>())
            {
                ((ISystemUnderTest) host).ToJson(new MyMessage { Name = "Jeremy" })
                    .ShouldNotContain(typeof(MyMessage).FullName);
            }
        }

        [Fact]
        public void customize_by_injecting_settings()
        {
            using (var host = SystemUnderTest.ForStartup<Startup>(builder =>
            {
                return builder.ConfigureServices(_ =>
                {
                    _.AddTransient<JsonSerializerSettings>(p => new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });
                });
            }))
            {



                host.As<ISystemUnderTest>().ToJson(new MyMessage {Name = "Jeremy"})
                    .ShouldContain(typeof(MyMessage).FullName);
            }
        }

        [Fact]
        public void customize_by_altering_settings()
        {
            using (var host = SystemUnderTest.ForStartup<Startup>())
            {
                host.JsonSerializerSettings.TypeNameHandling = TypeNameHandling.All;

                ((ISystemUnderTest) host).ToJson(new MyMessage {Name = "Jeremy"})
                    .ShouldContain(typeof(MyMessage).FullName);
            }
        }

        public class MyMessage
        {
            public string Name;
        }
    }


}