var builder = DistributedApplication.CreateBuilder(args);

var seq = builder.AddSeq("seq")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpoint("http", endpoint => endpoint.Port = 5341);

var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume();

var tododb = postgres.AddDatabase("tododb");

builder.AddProject<Projects.TodoApi>("todoapi")
    .WithReference(seq)
    .WaitFor(seq)
    .WithReference(tododb)
    .WaitFor(tododb);

builder.Build().Run();
