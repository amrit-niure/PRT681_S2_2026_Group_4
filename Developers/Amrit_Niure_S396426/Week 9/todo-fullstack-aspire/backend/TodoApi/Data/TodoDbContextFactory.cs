using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TodoApi.Data;

/// <summary>
/// Lets `dotnet ef migrations` build a <see cref="TodoDbContext"/> at design time, when the
/// Aspire-injected "tododb" connection string isn't available (it's only supplied by the
/// AppHost at run time). The connection string here is never used to actually connect.
/// </summary>
public class TodoDbContextFactory : IDesignTimeDbContextFactory<TodoDbContext>
{
    public TodoDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TodoDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=tododb;Username=postgres;Password=postgres");
        return new TodoDbContext(optionsBuilder.Options);
    }
}
