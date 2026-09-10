using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "frontend";


builder.Services.AddControllers();


builder.Services.AddOpenApi();


builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));


var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? ["http://localhost:5173", "http://localhost:5174"];
builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();


// Make sure the folder holding the SQLite file exists. On Azure App Service the
// database lives under /home (persistent storage), which is not present in the image.
var connectionString = builder.Configuration.GetConnectionString("Default");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    var dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;
    var dbDirectory = Path.GetDirectoryName(Path.GetFullPath(dataSource));
    if (!string.IsNullOrEmpty(dbDirectory))
    {
        Directory.CreateDirectory(dbDirectory);
    }
}

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

app.UseAuthorization();

app.MapControllers();

app.Run();
