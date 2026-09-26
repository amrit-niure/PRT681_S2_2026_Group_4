var builder = DistributedApplication.CreateBuilder(args);

var seq = builder.AddSeq("seq")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpoint("http", endpoint => endpoint.Port = 5342);

var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume();

var ticketingdb = postgres.AddDatabase("ticketingdb");

builder.AddProject<Projects.TicketingApi>("ticketingapi")
    .WithReference(seq)
    .WaitFor(seq)
    .WithReference(ticketingdb)
    .WaitFor(ticketingdb);

builder.Build().Run();
