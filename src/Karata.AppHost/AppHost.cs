using Microsoft.AspNetCore.Http;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres(
        name: "karata-cluster",
        userName: builder.AddParameter(name: "karata-cluster-username", secret: true),
        password: builder.AddParameter(name: "karata-cluster-password", secret: true)
    )
    .WithDataVolume(isReadOnly: false);

var cardsDatabase = postgres.AddDatabase("cards-db", "cards_db");

var cards = builder.AddProject<Projects.Karata_Cards>("cards")
    .WaitFor(cardsDatabase)
    .WithReference(cardsDatabase)
    .WithEnvironment("DATABASE_URL", cardsDatabase.Resource.UriExpression)
    .WithHttpHealthCheck("/health", StatusCodes.Status200OK);

var bot = builder.AddProject<Projects.Karata_Bot>("bot")
    .WithHttpHealthCheck("/health", StatusCodes.Status200OK)
    .WaitFor(cards)
    .WithReference(cards);

var web = builder.AddProject<Projects.Karata_Web>("web")
    .WithHttpHealthCheck("", StatusCodes.Status200OK)
    .WaitFor(cards)
    .WaitFor(bot)
    .WithReference(cards)
    .WithReference(bot);

var desktop = builder.AddProject<Projects.Karata_Desktop>("desktop")
    .WaitFor(cards)
    .WaitFor(bot)
    .WithReference(cards)
    .WithReference(bot);

builder.Build().Run();