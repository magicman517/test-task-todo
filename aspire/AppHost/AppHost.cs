var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres")
    .WithPgWeb()
    .WithDataVolume("todo-app-pg-data");

var database = postgres
    .AddDatabase("database", "todo-db");

var migrator = builder
    .AddProject<Projects.API>("migrator")
    .WithArgs("--migrate")
    .WithReference(database)
    .WaitFor(database);

var api = builder
    .AddProject<Projects.API>("api")
    // .WithHttpHealthCheck("/api/healthz")
    .WithReference(database)
    .WaitFor(database)
    .WaitForCompletion(migrator);

var web = builder
    .AddViteApp("web", "../../src/Web/", "start")
    .WithPnpm()
    .WithHttpEndpoint(10002)
    .WaitFor(api);

builder
    .AddYarp("gateway")
    .WithHostPort(8080)
    .WithConfiguration(cfg =>
    {
        cfg.AddRoute("/api/{**catch-all}", api);
        cfg.AddRoute(web);
    })
    .WaitFor(api);

await builder.Build().RunAsync();
