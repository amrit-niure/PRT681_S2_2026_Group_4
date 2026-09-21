var builder = DistributedApplication.CreateBuilder(args);

var seq = builder.AddSeq("seq")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpoint("http", endpoint => endpoint.Port = 5342);

var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume();

var workforcedb = postgres.AddDatabase("workforcedb");

builder.AddProject<Projects.WorkforceApi>("workforceapi")
    .WithReference(seq)
    .WaitFor(seq)
    .WithReference(workforcedb)
    .WaitFor(workforcedb);

builder.Build().Run();
