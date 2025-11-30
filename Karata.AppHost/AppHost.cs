var builder = DistributedApplication.CreateBuilder(args);

var server = builder.AddProject<Projects.Karata_Server>("server")
    .WithHttpHealthCheck("/health");

var bot = builder.AddProject<Projects.Karata_Bot>("bot")
    .WithHttpHealthCheck("/health")
    .WithReference(server)
    .WaitFor(server);

var desktop = builder.AddProject<Projects.Karata_Desktop>("desktop")
    .WithReference(server)
    .WaitFor(server);

builder.Build().Run();