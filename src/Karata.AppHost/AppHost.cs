using Microsoft.AspNetCore.Http;

var builder = DistributedApplication.CreateBuilder(args);

var clusterUsername = builder.AddParameter(name: "cluster-username", secret: true);
var clusterPassword = builder.AddParameter(name: "cluster-password", secret: true);
var idUsername = builder.AddParameter(name: "id-username");
var idPassword = builder.AddParameter(name: "id-password", secret: true);
var masSecret = builder.AddParameter("mas-secret", secret: true);

var postgres = builder.AddPostgres("cluster", clusterUsername, clusterPassword)
    .WithImage(image: "timescale/timescaledb-ha", tag: "pg18")
    .WithDataVolume(isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent);

var idDatabase = postgres.AddDatabase("id-db", "id_db");
var masDatabase = postgres.AddDatabase("mas-db", "mas_db");
var synapseDatabase = postgres.AddDatabase("synapse-db", "synapse_db")
    .WithCreationScript(
        "CREATE DATABASE synapse_db ENCODING 'UTF8' LC_COLLATE = 'C' LC_CTYPE = 'C' TEMPLATE template0;");
var cardsDatabase = postgres.AddDatabase("cards-db", "cards_db");
var triviaDatabase = postgres.AddDatabase("trivia-db", "trivia_db");
var platformDatabase = postgres.AddDatabase("platform-db", "platform_db");
var goDatabase = postgres.AddDatabase("go-db", "go_db");

var id = builder.AddKeycloak("id", 18080, idUsername, idPassword)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithReference(idDatabase)
    .WithEnvironment("KC_DB", "postgres")
    .WithEnvironment("KC_DB_USERNAME", postgres.Resource.UserNameParameter!)
    .WithEnvironment("KC_DB_PASSWORD", postgres.Resource.PasswordParameter)
    .WithEnvironment("KC_DB_URL", idDatabase.Resource.JdbcConnectionString)
    .WithRealmImport(import: "./configuration/keycloak/karata.json");

var mas = builder.AddContainer("mas", "ghcr.io/element-hq/matrix-authentication-service")
    .WaitFor(id)
    .WaitFor(masDatabase)
    .WithReference(id)
    .WithReference(masDatabase)
    .WithEnvironment("MAS_CONFIG", "/config/config.yaml")
    .WithHttpEndpoint(port: 18090, targetPort: 8080)
    .WithHttpEndpoint(port: 18091, targetPort: 8081, name: "internal")
    .WithEndpoint("internal", e => e.ExcludeReferenceEndpoint = true)
    .WithHttpHealthCheck("/health", StatusCodes.Status200OK, endpointName: "internal")
    .WithContainerFiles("/config", async (_, ct) =>
    {
        var username = Uri.EscapeDataString((await postgres.Resource.UserNameParameter!.GetValueAsync(ct))!);
        var passord = Uri.EscapeDataString((await postgres.Resource.PasswordParameter.GetValueAsync(ct))!);
        var uri = $"postgresql://{username}:{passord}@{postgres.Resource.Name}:5432/mas_db?sslmode=disable";

        var yaml = File.ReadAllText(Path.Combine(builder.AppHostDirectory, "configuration/mas/config.template.yml"))
            .Replace("{{MAS_PUBLIC_BASE}}", "http://localhost:18090/")
            .Replace("{{DATABASE_URL}}", uri)
            .Replace("{{HOMESERVER_NAME}}", "karata.app")
            .Replace("{{MAS_SYNAPSE_SHARED_SECRET}}", await masSecret.Resource.GetValueAsync(ct))
            .Replace("{{SYNAPSE_INTERNAL_URL}}", "http://synapse:18000/")
            .Replace("{{KEYCLOAK_INTERNAL_ISSUER}}", "http://id:8080/realms/karata")
            .Replace("{{KEYCLOAK_EXTERNAL_ISSUER}}", "http://localhost:18080/realms/karata");

        return [new ContainerFile { Name = "config.yaml", Contents = yaml }];
    });

var synapse = builder.AddContainer("synapse", "matrixdotorg/synapse")
    .WaitFor(mas)
    .WaitFor(synapseDatabase)
    .WithReference(synapseDatabase)
    .WithHttpEndpoint(port: 18000, targetPort: 18000)
    .WithHttpHealthCheck(path: "/health", statusCode: StatusCodes.Status200OK)
    .WithContainerFiles("/data", async (_, ct) =>
    {
        var yaml = File
            .ReadAllText(Path.Combine(builder.AppHostDirectory, "configuration/synapse/homeserver.template.yml"))
            .Replace("{{POSTGRES_HOST}}", postgres.Resource.Name)
            .Replace("{{POSTGRES_USER}}", await postgres.Resource.UserNameParameter!.GetValueAsync(ct))
            .Replace("{{POSTGRES_PASSWORD}}", await postgres.Resource.PasswordParameter.GetValueAsync(ct))
            .Replace("{{MAS_INTERNAL_URL}}", "http://mas:8080/")
            .Replace("{{MAS_SYNAPSE_SHARED_SECRET}}", await masSecret.Resource.GetValueAsync(ct));

        return
        [
            new ContainerFile { Name = "homeserver.yaml", Contents = yaml },
            new ContainerFile
            {
                Name = "signing.key",
                SourcePath = Path.Combine(builder.AppHostDirectory, "configuration/synapse/signing.key")
            },
            new ContainerFile
            {
                Name = "log.yml",
                SourcePath = Path.Combine(builder.AppHostDirectory, "configuration/synapse/log.yml")
            },
            new ContainerDirectory { Name = "media_store", Group = 991, Owner = 991 },
            new ContainerDirectory { Name = "uploads" }
        ];
    });

var element = builder.AddContainer("element", "vectorim/element-web")
    .WaitFor(synapse)
    .WithHttpEndpoint(targetPort: 80)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/", StatusCodes.Status200OK)
    .WithContainerFiles("/app", async (_, _) =>
    {
        const string json =
            """
            { 
                "default_server_config": { 
                    "m.homeserver": { 
                        "base_url": "http://localhost:18000/",
                        "server_name": "localhost"
                    }
                },
                "disable_guests": true
            }
            """;
        return [new ContainerFile { Name = "config.json", Contents = json }];
    });

var platform = builder.AddProject<Projects.Karata_Platform>("platform")
    .WaitFor(id)
    .WaitFor(platformDatabase)
    .WithReference(id)
    .WithReference(platformDatabase)
    .WithEnvironment("DATABASE_URL", platformDatabase.Resource.UriExpression)
    .WithEnvironment("Keycloak__realm", "karata")
    .WithEnvironment("Keycloak__auth-server-url", id.GetEndpoint("http"))
    .WithEnvironment("Keycloak__ssl-required", "none")
    .WithEnvironment("Keycloak__resource", "karata-platform")
    .WithEnvironment("Keycloak__verify-token-audience", true.ToString())
    .WithEnvironment("Keycloak__credentials__secret", Guid.Empty.ToString())
    .WithEnvironment("Keycloak__confidential-port", 0.ToString())
    .WithHttpHealthCheck("/health", StatusCodes.Status200OK);

var go = builder.AddProject<Projects.Karata_Go>("go")
    .WaitFor(id)
    .WaitFor(goDatabase)
    .WithReference(id)
    .WithReference(goDatabase)
    .WithEnvironment("DATABASE_URL", goDatabase.Resource.UriExpression)
    .WithEnvironment("Keycloak__realm", "karata")
    .WithEnvironment("Keycloak__auth-server-url", id.GetEndpoint("http"))
    .WithEnvironment("Keycloak__ssl-required", "none")
    .WithEnvironment("Keycloak__resource", "karata-go")
    .WithEnvironment("Keycloak__verify-token-audience", true.ToString())
    .WithEnvironment("Keycloak__credentials__secret", Guid.Empty.ToString())
    .WithEnvironment("Keycloak__confidential-port", 0.ToString())
    .WithHttpHealthCheck("/health", StatusCodes.Status200OK);

var cards = builder.AddProject<Projects.Karata_Cards>("cards")
    .WaitFor(id)
    .WaitFor(cardsDatabase)
    .WithReference(id)
    .WithReference(cardsDatabase)
    .WithEnvironment("DATABASE_URL", cardsDatabase.Resource.UriExpression)
    .WithEnvironment("KARATA_ID_AUDIENCE", "karata-platform")
    .WithEnvironment("KARATA_ID_AUTHORITY", $"{id.GetEndpoint("http")}/realms/karata")
    .WithEnvironment("KARATA_ID_CLIENT_ID", "karata-cards")
    .WithEnvironment("KARATA_ID_CLIENT_SECRET", Guid.Empty.ToString())
    .WithEnvironment("PLATFORM_URL", platform.GetEndpoint("https"))
    .WithEnvironment("Keycloak__realm", "karata")
    .WithEnvironment("Keycloak__auth-server-url", id.GetEndpoint("http"))
    .WithEnvironment("Keycloak__ssl-required", "none")
    .WithEnvironment("Keycloak__resource", "karata-cards")
    .WithEnvironment("Keycloak__verify-token-audience", true.ToString())
    .WithEnvironment("Keycloak__credentials__secret", Guid.Empty.ToString())
    .WithEnvironment("Keycloak__confidential-port", 0.ToString())
    .WithHttpHealthCheck("/health", StatusCodes.Status200OK);

var trivia = builder.AddProject<Projects.Karata_Trivia>("trivia")
    .WaitFor(id)
    .WaitFor(triviaDatabase)
    .WithReference(id)
    .WithReference(triviaDatabase)
    .WithEnvironment("DATABASE_URL", triviaDatabase.Resource.UriExpression)
    .WithEnvironment("KARATA_ID_AUDIENCE", "karata-platform")
    .WithEnvironment("KARATA_ID_AUTHORITY", $"{id.GetEndpoint("http")}/realms/karata")
    .WithEnvironment("KARATA_ID_CLIENT_ID", "karata-trivia")
    .WithEnvironment("KARATA_ID_CLIENT_SECRET", Guid.Empty.ToString())
    .WithEnvironment("PLATFORM_URL", platform.GetEndpoint("https"))
    .WithEnvironment("Keycloak__realm", "karata")
    .WithEnvironment("Keycloak__auth-server-url", id.GetEndpoint("http"))
    .WithEnvironment("Keycloak__ssl-required", "none")
    .WithEnvironment("Keycloak__resource", "karata-trivia")
    .WithEnvironment("Keycloak__verify-token-audience", true.ToString())
    .WithEnvironment("Keycloak__credentials__secret", Guid.Empty.ToString())
    .WithEnvironment("Keycloak__confidential-port", 0.ToString())
    .WithHttpHealthCheck("/health", StatusCodes.Status200OK);

var bot = builder.AddProject<Projects.Karata_Bot>("bot")
    .WaitFor(id)
    .WithReference(id)
    .WithReference(cards)
    .WithEnvironment("KARATA_CARDS_HOST", cards.GetEndpoint("https"))
    .WithEnvironment("KARATA_PLATFORM_HOST", platform.GetEndpoint("https"))
    .WithEnvironment("KARATA_ID_AUTHORITY", $"{id.GetEndpoint("http")}/realms/karata")
    .WithEnvironment("KARATA_ID_CLIENT_ID", "karata-bot")
    .WithEnvironment("KARATA_ID_CLIENT_SECRET", Guid.Empty.ToString())
    .WithEnvironment("KARATA_ID_SCOPE", "openid profile email karata-cards karata-platform")
    .WithHttpHealthCheck("/health", StatusCodes.Status200OK);

var web = builder.AddProject<Projects.Karata_Web>("web")
    .WaitFor(id)
    .WithReference(id)
    .WithReference(cards)
    .WithReference(bot)
    .WithHttpHealthCheck("/", StatusCodes.Status200OK);

var desktop = builder.AddProject<Projects.Karata_Desktop>("desktop")
    .WaitFor(id)
    .WithReference(id)
    .WithReference(cards)
    .WithReference(bot);

builder.Build().Run();