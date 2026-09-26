using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TicketingApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Structured logging: every ILogger<T> call flows through Serilog to the console and to
// Seq. Aspire injects the "seq" connection string; the fallback is a local Seq.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "TicketingApi")
    .WriteTo.Console()
    .WriteTo.Seq(context.Configuration.GetConnectionString("seq") ?? "http://localhost:5342"));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<TicketingDbContext>("ticketingdb");

// Authentication & authorization: ASP.NET Core Identity with bearer-token API endpoints.
builder.Services.AddAuthorization();
builder.Services
    .AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<TicketingDbContext>();

var app = builder.Build();

// One structured "HTTP GET /api/tickets responded 200 in 12 ms" event per request.
app.UseSerilogRequestLogging();

app.MapDefaultEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();
    db.Database.Migrate();
}

app.MapOpenApi();

app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/api/auth").MapIdentityApi<IdentityUser>();

app.MapControllers();

app.Run();
