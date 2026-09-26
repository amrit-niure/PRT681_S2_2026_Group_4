var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.TicketingApi>("ticketingapi");

builder.Build().Run();
