// Two ways to run the whole system from this AppHost:
//
//   dotnet run --project TicketingApi.AppHost
//       API runs as a project (debuggable) and the frontend as the Vite dev server.
//
//   dotnet run --project TicketingApi.AppHost -- --Containers:Enabled=true
//       API and frontend are built from their Dockerfiles and run as containers, exactly
//       like `docker compose up`, but orchestrated (and observed) by Aspire.
//
// Postgres, Seq and Temporal are containers in both modes.

const int ApiPort = 5260;
const int FrontendPort = 5177;

var builder = DistributedApplication.CreateBuilder(args);
var useContainers = bool.TryParse(builder.Configuration["Containers:Enabled"], out var enabled) && enabled;

// Plain strings (not interpolated inline) so they are not treated as Aspire expressions.
string apiUrl = $"http://localhost:{ApiPort}";
string frontendUrl = $"http://localhost:{FrontendPort}";

var seq = builder.AddSeq("seq")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpoint("http", endpoint => endpoint.Port = 5342);

// Temporal dev server (gRPC on 7233, web UI on 8233) with its own embedded state.
var temporal = builder.AddContainer("temporal", "temporalio/temporal", "latest")
    .WithArgs("server", "start-dev", "--ip", "0.0.0.0")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpoint(port: 7233, targetPort: 7233, name: "grpc", scheme: "http")
    .WithHttpEndpoint(port: 8233, targetPort: 8233, name: "ui");

var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume();

var ticketingdb = postgres.AddDatabase("ticketingdb");

// Settings shared by both API modes. Secrets (the Resend API key, the Exceptionless key)
// come from the AppHost's own configuration, e.g.
//   dotnet user-secrets set Smtp:Password re_xxx --project TicketingApi.AppHost
IResourceBuilder<T> ConfigureApi<T>(IResourceBuilder<T> api) where T : IResourceWithEnvironment, IResourceWithWaitSupport
{
    api.WithReference(seq).WaitFor(seq)
        .WithReference(ticketingdb).WaitFor(ticketingdb)
        .WithEnvironment("Temporal__Address", temporal.GetEndpoint("grpc").Property(EndpointProperty.HostAndPort))
        .WaitFor(temporal)
        .WithEnvironment("Cors__AllowedOrigins__0", frontendUrl);

    foreach (var key in new[] { "Smtp:Password", "Smtp:FromAddress", "Exceptionless:ApiKey" })
    {
        if (builder.Configuration[key] is { Length: > 0 } value)
        {
            api.WithEnvironment(key.Replace(":", "__"), value);
        }
    }

    return api;
}

if (useContainers)
{
    // Same Dockerfiles as docker-compose. The Dockerfile's own HEALTHCHECK is honoured by
    // Docker; WithHttpHealthCheck makes Aspire's dashboard show the state too.
    var api = ConfigureApi(builder.AddDockerfile("ticketingapi", "..", "Dockerfile")
        .WithHttpEndpoint(port: ApiPort, targetPort: 8080, name: "http")
        .WithHttpHealthCheck("/alive")
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Production")
        .WithEnvironment("Diagnostics__Enabled", "true")
        .WithEnvironment("Elmah__AllowAnonymous", "true")
        .WithVolume("ticketing-dpkeys", "/app/dp-keys")
        .WithVolume("ticketing-elmah", "/app/elmah-logs"));

    // Vite inlines the API URL at build time, and the browser (not the container) calls it.
    builder.AddDockerfile("frontend", "../../frontend", "Dockerfile")
        .WithBuildArg("VITE_API_URL", apiUrl)
        .WithHttpEndpoint(port: FrontendPort, targetPort: 80, name: "http")
        .WithHttpHealthCheck("/healthz")
        .WaitFor(api);
}
else
{
    var api = ConfigureApi(builder.AddProject<Projects.TicketingApi>("ticketingapi"));

    builder.AddViteApp("frontend", "../../frontend")
        .WithEnvironment("VITE_API_URL", apiUrl)
        .WithEndpoint("http", endpoint => endpoint.Port = FrontendPort)
        .WaitFor(api);
}

builder.Build().Run();
