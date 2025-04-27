# Snapshot Testing

Although Alba does not ship built-in snapshotting support, it does easily integrate with the popular [Verify](https://github.com/VerifyTests/Verify) framework.
Given Verify and [Verify.AspNetCore](https://github.com/VerifyTests/Verify.AspNetCore) has been added and initialized for your test framework of choice, you can reliably feed in the `HttpContext` and response data for a complete picture of your network request.

Given a test with the shape of:
<!-- snippet: sample_snapshot_testing -->
<a id='snippet-sample_snapshot_testing'></a>
```cs
await using var host = await AlbaHost.For<global::Program>();

var scenario = await host.Scenario(s =>
{
    s.Post.Json(new MyEntity(Guid.NewGuid(), "SomeValue")).ToUrl("/json");
});

var body = scenario.ReadAsJson<MyEntity>();

await Verify(new {
    scenario.Context,
    ResponseBody = body,
});
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/SnapshotTesting.cs#L17-L31' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_snapshot_testing' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Will result in a snapshot of:

<<< ../../src/Alba.Testing/Samples/SnapshotTesting.SnapshotTest.verified.txt{json}
