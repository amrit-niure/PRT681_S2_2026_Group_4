using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TicketingApi.Data;

public class TicketingDbContextFactory : IDesignTimeDbContextFactory<TicketingDbContext>
{
    public TicketingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TicketingDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=ticketingdb;Username=postgres;Password=postgres");
        return new TicketingDbContext(optionsBuilder.Options);
    }
}
