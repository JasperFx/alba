var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgresdb").AddDatabase("apiservicedb");

builder
    .AddProject<Projects.AspireApp_ApiService>("apiservice")
    .WithReference(postgres);

builder.Build().Run();