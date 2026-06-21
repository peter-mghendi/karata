var builder = DistributedApplication.CreateBuilder(args);

var cards = builder.AddProject<Projects.Karata_Cards>("cards")
    .WithHttpHealthCheck("/health");
var bot = builder.AddProject<Projects.Karata_Bot>("bot")
    .WithHttpHealthCheck("/health")
    .WaitFor(cards)
    .WithReference(cards);
var desktop = builder.AddProject<Projects.Karata_Desktop>("desktop")
    .WaitFor(cards)
    .WaitFor(bot)
    .WithReference(cards);

builder.Build().Run();