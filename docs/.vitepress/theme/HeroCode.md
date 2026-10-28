```cs
await using var host = await AlbaHost.For<Program>()
    .WithConfiguration("ConnectionStrings:Db", "Database=test");

await host.Scenario(_ =>
{
    _.Post.Json(new Order("A-1", 2)).ToUrl("/orders");
    _.StatusCodeShouldBe(201);
    _.JsonPath("$.sku").ShouldBe("A-1");
});
```
