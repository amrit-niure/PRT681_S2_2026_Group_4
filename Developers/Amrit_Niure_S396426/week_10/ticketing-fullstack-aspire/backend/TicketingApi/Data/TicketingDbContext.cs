using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TicketingApi.Models;

namespace TicketingApi.Data;

/// <summary>
/// EF Core unit of work for the ticketing database. Also hosts the ASP.NET Core Identity
/// tables (AspNetUsers, AspNetRoles, ...). One instance is created per HTTP request.
/// </summary>
public class TicketingDbContext : IdentityDbContext<IdentityUser>
{
    public TicketingDbContext(DbContextOptions<TicketingDbContext> options) : base(options)
    {
    }

    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<TicketComment> TicketComments => Set<TicketComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Npgsql's "timestamp without time zone" mapping rejects DateTime.Kind == Utc
        // (it wants Unspecified), so strip the Kind on the way in/out.
        var unspecifiedKind = new ValueConverter<DateTime, DateTime>(
            v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified),
            v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified));

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Description).IsRequired().HasMaxLength(4000);
            entity.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(t => t.CreatedByUserId).IsRequired();
            entity.Property(t => t.CreatedAt).HasColumnType("timestamp without time zone")
                .HasConversion(unspecifiedKind);
            entity.Property(t => t.UpdatedAt).HasColumnType("timestamp without time zone")
                .HasConversion(unspecifiedKind);
            entity.HasIndex(t => t.CreatedByUserId);
            entity.HasIndex(t => t.Status);
        });

        modelBuilder.Entity<TicketComment>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Body).IsRequired().HasMaxLength(4000);
            entity.Property(c => c.AuthorUserId).IsRequired();
            entity.Property(c => c.CreatedAt).HasColumnType("timestamp without time zone")
                .HasConversion(unspecifiedKind);
            entity.HasOne(c => c.Ticket)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
