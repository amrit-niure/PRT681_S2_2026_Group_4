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

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "TicketingApi")
    .WriteTo.Console()
    .WriteTo.Seq(context.Configuration.GetConnectionString("seq") ?? "http://localhost:5342"));

const string FrontendCorsPolicy = "frontend";

var keysDirectory = Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "dp-keys"));
builder.Services.AddDataProtection().PersistKeysToFileSystem(keysDirectory);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<TicketingDbContext>("ticketingdb");

builder.Services.AddAuthorization();
builder.Services
    .AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<TicketingDbContext>();

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

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

var elmahAllowAnonymous = builder.Configuration.GetValue<bool>("Elmah:AllowAnonymous");
builder.Services.AddElmah<XmlFileErrorLog>(options =>
{
    options.Path = "/elmah";
    options.LogPath = builder.Configuration["Elmah:LogPath"] ?? "./elmah-logs";
    options.ApplicationName = "TicketingApi";
    options.OnPermissionCheck = context =>
        elmahAllowAnonymous || builder.Environment.IsDevelopment() || context.User.Identity?.IsAuthenticated == true;
});

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

if (app.Configuration.GetValue<bool>("HttpsRedirection:Enabled"))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/api/auth").MapIdentityApi<IdentityUser>();

app.MapControllers();

app.Run();
