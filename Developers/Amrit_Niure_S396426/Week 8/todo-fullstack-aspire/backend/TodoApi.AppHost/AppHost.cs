var builder = DistributedApplication.CreateBuilder(args);

var seq = builder.AddSeq("seq")
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<Projects.TodoApi>("todoapi")
    .WithReference(seq)
    .WaitFor(seq);

builder.Build().Run();
