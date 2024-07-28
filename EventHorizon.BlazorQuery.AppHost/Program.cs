var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.EventHorizon_BlazorQuery_ApiService>("apiservice");

builder
    .AddProject<Projects.EventHorizon_BlazorQuery_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
