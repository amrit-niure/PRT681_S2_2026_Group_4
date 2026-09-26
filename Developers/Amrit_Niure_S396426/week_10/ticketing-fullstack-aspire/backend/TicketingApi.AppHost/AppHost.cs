var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume();

var ticketingdb = postgres.AddDatabase("ticketingdb");

builder.AddProject<Projects.TicketingApi>("ticketingapi")
    .WithReference(ticketingdb)
    .WaitFor(ticketingdb);

builder.Build().Run();
