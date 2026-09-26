var builder = DistributedApplication.CreateBuilder(args);

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

builder.AddProject<Projects.TicketingApi>("ticketingapi")
    .WithReference(seq)
    .WaitFor(seq)
    .WithReference(ticketingdb)
    .WaitFor(ticketingdb)
    .WithEnvironment("Temporal__Address", temporal.GetEndpoint("grpc"))
    .WaitFor(temporal);

builder.Build().Run();
