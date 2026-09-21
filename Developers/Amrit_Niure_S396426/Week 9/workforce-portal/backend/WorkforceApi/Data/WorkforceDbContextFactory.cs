using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WorkforceApi.Data;

/// <summary>
/// Lets `dotnet ef migrations` build a <see cref="WorkforceDbContext"/> at design time, when the
/// Aspire-injected "workforcedb" connection string isn't available (it's only supplied by the
/// AppHost at run time). The connection string here is never used to actually connect.
/// </summary>
public class WorkforceDbContextFactory : IDesignTimeDbContextFactory<WorkforceDbContext>
{
    public WorkforceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WorkforceDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=workforcedb;Username=postgres;Password=postgres");
        return new WorkforceDbContext(optionsBuilder.Options);
    }
}
