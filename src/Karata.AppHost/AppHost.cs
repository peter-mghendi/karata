var builder = DistributedApplication.CreateBuilder(args);

var server = builder.AddProject<Projects.Karata_Server>("server")
    .WithHttpHealthCheck("/health");
var bot = builder.AddProject<Projects.Karata_Bot>("bot")
    .WithHttpHealthCheck("/health")
    .WaitFor(server)
    .WithReference(server);
var desktop = builder.AddProject<Projects.Karata_Desktop>("desktop")
    .WaitFor(server)
    .WithReference(server);

builder.Build().Run();