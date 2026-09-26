using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using ElmahCore;
using ElmahCore.Mvc;
using Exceptionless;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Temporalio.Extensions.Hosting;
using TicketingApi.Data;
using TicketingApi.Notifications;
using TicketingApi.Workflows;

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

const string FrontendCorsPolicy = "frontend";

// Persist data-protection keys (used to sign auth tokens) next to the app so a restart
// doesn't sign everyone out. In a container, mount a volume at /app/dp-keys.
var keysDirectory = Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "dp-keys"));
builder.Services.AddDataProtection().PersistKeysToFileSystem(keysDirectory);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<TicketingDbContext>("ticketingdb");

// Authentication & authorization: ASP.NET Core Identity with bearer-token API endpoints.
builder.Services.AddAuthorization();
builder.Services
    .AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<TicketingDbContext>();

// Outgoing email over SMTP (Resend relay by default).
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

// Temporal: the API starts email workflows; an in-process worker executes them.
var temporal = builder.Configuration.GetSection(TemporalOptions.SectionName).Get<TemporalOptions>() ?? new();
builder.Services.Configure<TemporalOptions>(builder.Configuration.GetSection(TemporalOptions.SectionName));
builder.Services.AddTemporalClient(options =>
{
    options.TargetHost = temporal.Address;
    options.Namespace = temporal.Namespace;
});
builder.Services
    .AddHostedTemporalWorker(temporal.TaskQueue)
    .AddScopedActivities<TicketEmailActivities>()
    .AddWorkflow<TicketEmailWorkflow>();
builder.Services.AddSingleton<TicketEmailDispatcher>();

// ELMAH: records every unhandled exception (with request context) and serves a UI at /elmah.
// The UI is open in Development; elsewhere it needs a signed-in user unless Elmah:AllowAnonymous is set.
var elmahAllowAnonymous = builder.Configuration.GetValue<bool>("Elmah:AllowAnonymous");
builder.Services.AddElmah<XmlFileErrorLog>(options =>
{
    options.Path = "/elmah";
    options.LogPath = builder.Configuration["Elmah:LogPath"] ?? "./elmah-logs";
    options.ApplicationName = "TicketingApi";
    options.OnPermissionCheck = context =>
        elmahAllowAnonymous || builder.Environment.IsDevelopment() || context.User.Identity?.IsAuthenticated == true;
});

// Exceptionless: ships unhandled exceptions to an Exceptionless server (cloud or self-hosted).
// Only active when Exceptionless:ApiKey is set, so local runs work without an account.
var exceptionlessEnabled = !string.IsNullOrWhiteSpace(builder.Configuration["Exceptionless:ApiKey"]);
if (exceptionlessEnabled)
{
    builder.Services.AddExceptionless(builder.Configuration);
}

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? ["http://localhost:5177"];
builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownIPNetworks = { },
    KnownProxies = { }
});

if (exceptionlessEnabled)
{
    app.UseExceptionless();
}

// One structured "HTTP GET /api/tickets responded 200 in 12 ms" event per request.
app.UseSerilogRequestLogging();

app.UseElmah();

app.MapDefaultEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();
    db.Database.Migrate();
}

app.MapOpenApi();
app.MapScalarApiReference();
app.MapGet("/", () => Results.Redirect("/scalar/v1"));

app.UseCors(FrontendCorsPolicy);

// Off by default: behind a TLS-terminating proxy (or a local container) plain HTTP is expected.
if (app.Configuration.GetValue<bool>("HttpsRedirection:Enabled"))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/api/auth").MapIdentityApi<IdentityUser>();

app.MapControllers();

app.Run();
