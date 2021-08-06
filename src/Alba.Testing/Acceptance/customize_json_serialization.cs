using System;
using System.Net;
using System.Threading.Tasks;
using Baseline;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                ((IAlbaTestHost) host).ToJson(new MyMessage { Name = "Jeremy" })
                    .ShouldNotContain(typeof(MyMessage).FullName);
            }
        }

        [Fact]
        public void customize_by_configuring_settings()
        {
            using (var host = SystemUnderTest.ForStartup<Startup>(builder =>
            {
                return builder.ConfigureServices((c, _) =>
                {
                    _.Configure<MvcNewtonsoftJsonOptions>(o =>
                    {
                        o.SerializerSettings.TypeNameHandling = TypeNameHandling.All;
                    });
                });
            }))
            {
                
                host.As<IAlbaTestHost>().ToJson(new MyMessage {Name = "Jeremy"})
                    .ShouldContain(typeof(MyMessage).FullName);
            }
        }

        [Fact]
        public void customize_by_altering_settings_directly()
        {
            using (var host = SystemUnderTest.ForStartup<Startup>())
            {
                host.JsonSerializerSettings.TypeNameHandling = TypeNameHandling.All;

                ((IAlbaTestHost) host).ToJson(new MyMessage {Name = "Jeremy"})
                    .ShouldContain(typeof(MyMessage).FullName);
            }
        }

        public class MyMessage
        {
            public string Name;
        }
    }


}