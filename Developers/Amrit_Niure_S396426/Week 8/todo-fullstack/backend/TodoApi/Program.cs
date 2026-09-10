using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Notifications;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "frontend";

// Work out where the SQLite file lives so we can also keep the data-protection keys
// (used to sign auth tokens) next to it. On Azure App Service this folder is /home,
// which persists across restarts; without this every restart would sign everyone out.
var connectionString = builder.Configuration.GetConnectionString("Default");
var dbDirectory = ResolveDbDirectory(connectionString);
if (!string.IsNullOrEmpty(dbDirectory))
{
    Directory.CreateDirectory(dbDirectory);
    var keysDirectory = Directory.CreateDirectory(Path.Combine(dbDirectory, "dp-keys"));
    builder.Services.AddDataProtection().PersistKeysToFileSystem(keysDirectory);
}


builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlite(connectionString));


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


static string? ResolveDbDirectory(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return null;
    }

    var dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;
    return Path.GetDirectoryName(Path.GetFullPath(dataSource));
}
