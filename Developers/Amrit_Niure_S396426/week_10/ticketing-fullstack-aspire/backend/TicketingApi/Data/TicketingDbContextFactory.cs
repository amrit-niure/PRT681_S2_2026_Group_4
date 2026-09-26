using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TicketingApi.Data;

/// <summary>
/// Lets `dotnet ef migrations` build a <see cref="TicketingDbContext"/> at design time, when the
/// Aspire-injected "ticketingdb" connection string isn't available (it's only supplied by the
/// AppHost at run time). The connection string here is never used to actually connect.
/// </summary>
public class TicketingDbContextFactory : IDesignTimeDbContextFactory<TicketingDbContext>
{
    public TicketingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TicketingDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=ticketingdb;Username=postgres;Password=postgres");
        return new TicketingDbContext(optionsBuilder.Options);
    }
}
