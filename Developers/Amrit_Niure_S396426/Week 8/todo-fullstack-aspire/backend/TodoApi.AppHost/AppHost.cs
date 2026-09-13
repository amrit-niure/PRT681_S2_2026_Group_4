var builder = DistributedApplication.CreateBuilder(args);

var seq = builder.AddSeq("seq")
    .WithLifetime(ContainerLifetime.Persistent);

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
