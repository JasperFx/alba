#region sample_aspire_apphost
var builder = DistributedApplication.CreateBuilder(args);

var ledger = builder.AddPostgres("postgres").AddDatabase("ledger");
var messaging = builder.AddRabbitMQ("messaging");

// WithReference hands the API ConnectionStrings:ledger and ConnectionStrings:messaging
builder.AddProject<Projects.AspireSamples_Api>("api")
    .WithReference(ledger).WaitFor(ledger)
    .WithReference(messaging).WaitFor(messaging);

builder.Build().Run();
#endregion
