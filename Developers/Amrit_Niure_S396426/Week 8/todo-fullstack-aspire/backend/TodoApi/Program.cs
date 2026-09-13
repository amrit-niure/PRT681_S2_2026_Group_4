using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TodoApi.Data;
using TodoApi.Notifications;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq(context.Configuration.GetConnectionString("seq") ?? "http://localhost:5341"));

const string FrontendCorsPolicy = "frontend";

// Persist data-protection keys (used to sign auth tokens) next to the app so a restart
// doesn't sign everyone out. On Azure App Service this folder is /home, which persists
// across restarts.
var keysDirectory = Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "dp-keys"));
builder.Services.AddDataProtection().PersistKeysToFileSystem(keysDirectory);


builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<TodoDbContext>("tododb");


// Authentication & authorization: ASP.NET Core Identity with bearer-token API endpoints.
builder.Services.AddAuthorization();
builder.Services
    .AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<TodoDbContext>();


var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? ["http://localhost:5173", "http://localhost:5174"];
builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));


// Email reminders for overdue tasks.
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
builder.Services.Configure<ReminderOptions>(builder.Configuration.GetSection(ReminderOptions.SectionName));
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<OverdueReminderScanner>();
builder.Services.AddHostedService<DueTaskReminderService>();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapDefaultEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    db.Database.Migrate();
}


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseCors(FrontendCorsPolicy);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// Identity endpoints: POST /api/auth/register, /api/auth/login, /api/auth/refresh, ...
app.MapGroup("/api/auth").MapIdentityApi<IdentityUser>();

app.MapControllers();

app.Run();
