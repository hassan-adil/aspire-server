using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var dbUsername = builder.AddParameter("dbUsername", secret: true);
var dbPassword = builder.AddParameter("dbPassword", secret: true);
var kcUsername = builder.AddParameter("kcUsername", secret: true);
var kcPassword = builder.AddParameter("kcPassword", secret: true);
var mqUsername = builder.AddParameter("mqUsername", secret: true);
var mqPassword = builder.AddParameter("mqPassword", secret: true);
var portKeycloak = builder.AddParameter("portKeycloak", secret: false);
var portRedis = builder.AddParameter("portRedis", secret: false);
var portRabbitMq = builder.AddParameter("portRabbitMq", secret: false);

if (!int.TryParse(await portKeycloak.Resource.GetValueAsync(default), out var keycloakPort))
    throw new InvalidOperationException("Invalid portKeycloak parameter value.");

if (!int.TryParse(await portRedis.Resource.GetValueAsync(default), out var redisPort))
    throw new InvalidOperationException("Invalid portRedis parameter value.");

if (!int.TryParse(await portRabbitMq.Resource.GetValueAsync(default), out var rabbitMqPort))
    throw new InvalidOperationException("Invalid portRedis parameter value.");

// PostgreSQL Server
var postgresServer = builder.AddPostgres("postgres", dbUsername, dbPassword)
    .WithDataVolume(isReadOnly: false, name: "aspire_postgres_data")
    .WithPgWeb()
    .WithArgs("-c", "max_connections=300");

var loggerDb = postgresServer.AddDatabase("logger-db");
var userDb = postgresServer.AddDatabase("user-db");

var redis = builder.AddRedis("redis", redisPort);

// Add RabbitMQ server
var messaging = builder.AddRabbitMQ("messaging", mqUsername, mqPassword, rabbitMqPort)
    .WithManagementPlugin();

// Keycloak Identity Provider
var keycloak = builder.AddKeycloak("keycloak", port: keycloakPort, adminUsername: kcUsername, adminPassword: kcPassword)
                      .WithDataVolume("aspire_keycloak_data")
                      .WithRealmImport("./Realms");

builder.AddProject<Projects.Auth_API>("auth-api")
    .WaitFor(keycloak)
    .WithReference(redis)
    .WithReference(messaging)
    .WithReference(keycloak);

builder.AddProject<Projects.Logger_API>("logger-api")
    .WaitFor(postgresServer)
    .WithReference(loggerDb)
    .WithReference(messaging);

builder.AddProject<Projects.User_API>("user-api")
    .WaitFor(postgresServer)
    .WaitFor(keycloak)
    .WithReference(redis)
    .WithReference(userDb)
    .WithReference(messaging)
    .WithReference(keycloak); ;

builder.Build().Run();
