using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Alba.Internal;
using Xunit;

namespace Alba.Testing;

public class ActivityTests
{
    [Fact]
    public async Task ActivityTagged_AsExpected()
    {
        await using var host = await AlbaHost.For<Program>();
        var startCalled = false;
        var endCalled = false;
        var expectedTags = new []
        {
            AlbaTracing.HttpMethod,
            AlbaTracing.HttpUrl,
            AlbaTracing.HttpStatusCode,
            AlbaTracing.NetPeerName,
            AlbaTracing.HttpResponseContentLength,
            AlbaTracing.HttpRequestContentLength
        };
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => _.Name == "Alba",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = activity =>
            {
                startCalled = true;
                Assert.NotNull(activity);
                Assert.Equal($"POST /json", activity.DisplayName);
            },
            ActivityStopped = activity =>
            {
                endCalled = true;
                Assert.NotNull(activity);
                Assert.Contains(activity.Tags, x => expectedTags.Contains(x.Key));
                
            }
        };
        
        ActivitySource.AddActivityListener(listener);
        
        await host.Scenario(_ =>
        {
            _.Post.Json(new MyEntity(Guid.NewGuid())).ToUrl("/json");
        });
        
        Assert.True(startCalled);
        Assert.True(endCalled);
    }
}