using Microsoft.AspNetCore.Http;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres(
        name: "cluster",
        userName: builder.AddParameter(name: "karata-cluster-username", secret: true),
        password: builder.AddParameter(name: "karata-cluster-password", secret: true)
    )
    .WithDataVolume(isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent);

var idDatabase = postgres.AddDatabase("id-db", "id_db");
var cardsDatabase = postgres.AddDatabase("cards-db", "cards_db");
var platformDatabase = postgres.AddDatabase("platform-db", "platform_db");

var keycloak = builder.AddKeycloak(
        name: "id",
        port: 18080,
        adminUsername: builder.AddParameter(name: "karata-id-username"),
        adminPassword: builder.AddParameter(name: "karata-id-password", secret: true)
    )
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithReference(idDatabase)
    .WithEnvironment("KC_DB", "postgres")
    .WithEnvironment("KC_DB_USERNAME", postgres.Resource.UserNameParameter!)
    .WithEnvironment("KC_DB_PASSWORD", postgres.Resource.PasswordParameter)
    .WithEnvironment("KC_DB_URL", idDatabase.Resource.JdbcConnectionString)
    .WithRealmImport(import: "./karata.json");

var cards = builder.AddProject<Projects.Karata_Cards>("cards")
    .WaitFor(keycloak)
    .WaitFor(cardsDatabase)
    .WithReference(keycloak)
    .WithReference(cardsDatabase)
    .WithEnvironment("DATABASE_URL", cardsDatabase.Resource.UriExpression)
    .WithEnvironment("Keycloak__realm", "karata")
    .WithEnvironment("Keycloak__auth-server-url", keycloak.GetEndpoint("http"))
    .WithEnvironment("Keycloak__ssl-required", "none")
    .WithEnvironment("Keycloak__resource", "karata-cards")
    .WithEnvironment("Keycloak__verify-token-audience", false.ToString())
    .WithEnvironment("Keycloak__credentials__secret", Guid.Empty.ToString())
    .WithEnvironment("Keycloak__confidential-port", 0.ToString())
    .WithHttpHealthCheck("/health", StatusCodes.Status200OK);

var bot = builder.AddProject<Projects.Karata_Bot>("bot")
    .WaitFor(keycloak)
    .WaitFor(cards)
    .WithReference(keycloak)
    .WithReference(cards)
    .WithEnvironment("KARATA_CARDS_HOST", cards.GetEndpoint("https"))
    .WithEnvironment("Keycloak__Authority", $"{keycloak.GetEndpoint("http")}/realms/karata")
    .WithEnvironment("Keycloak__ClientId", "karata-bot")
    .WithEnvironment("Keycloak__ClientSecret", Guid.Empty.ToString())
    .WithEnvironment("Keycloak__Scope", "openid profile email")
    .WithHttpHealthCheck("/health", StatusCodes.Status200OK);

var platform = builder.AddProject<Projects.Karata_Platform>("platform")
    .WaitFor(keycloak)
    .WaitFor(platformDatabase)
    .WithReference(keycloak)
    .WithReference(platformDatabase)
    .WithEnvironment("DATABASE_URL", platformDatabase.Resource.UriExpression)
    .WithEnvironment("Keycloak__realm", "karata")
    .WithEnvironment("Keycloak__auth-server-url", keycloak.GetEndpoint("http"))
    .WithEnvironment("Keycloak__ssl-required", "none")
    .WithEnvironment("Keycloak__resource", "karata-platform")
    .WithEnvironment("Keycloak__verify-token-audience", false.ToString())
    .WithEnvironment("Keycloak__credentials__secret", Guid.Empty.ToString())
    .WithEnvironment("Keycloak__confidential-port", 0.ToString())
    .WithHttpHealthCheck("/health", StatusCodes.Status200OK);

var web = builder.AddProject<Projects.Karata_Web>("web")
    .WithHttpHealthCheck("/", StatusCodes.Status200OK)
    .WaitFor(keycloak)
    .WaitFor(cards)
    .WaitFor(bot)
    .WithReference(keycloak)
    .WithReference(cards)
    .WithReference(bot);

var desktop = builder.AddProject<Projects.Karata_Desktop>("desktop")
    .WaitFor(keycloak)
    .WaitFor(cards)
    .WaitFor(bot)
    .WithReference(keycloak)
    .WithReference(cards)
    .WithReference(bot);

builder.Build().Run();