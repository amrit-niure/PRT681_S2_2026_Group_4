using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TodoApi.Models;

namespace TodoApi.Data;

/// <summary>
/// EF Core unit of work for the to-do database. Also hosts the ASP.NET Core Identity
/// tables (AspNetUsers, AspNetRoles, ...). One instance is created per HTTP request.
/// </summary>
public class TodoDbContext : IdentityDbContext<IdentityUser>
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Npgsql's "timestamp without time zone" mapping rejects DateTime.Kind == Utc
        // (it wants Unspecified). DueDate/CreatedAt are plain calendar values, not tied
        // to a zone, so strip the Kind on the way in/out - EF applies this converter to
        // query parameters too (e.g. comparisons in OverdueReminderScanner), not just
        // property assignments.
        var unspecifiedKind = new ValueConverter<DateTime, DateTime>(
            v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified),
            v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified));
        var unspecifiedKindNullable = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Unspecified) : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Unspecified) : v);

        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
            entity.Property(t => t.CreatedAt).IsRequired()
                .HasColumnType("timestamp without time zone")
                .HasConversion(unspecifiedKind);
            entity.Property(t => t.DueDate)
                .HasColumnType("timestamp without time zone")
                .HasConversion(unspecifiedKindNullable);
            entity.Property(t => t.UserId).IsRequired();
            entity.HasIndex(t => t.UserId);
        });
    }
}
