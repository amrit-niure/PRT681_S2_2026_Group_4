using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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

        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
            entity.Property(t => t.CreatedAt).IsRequired();
            entity.Property(t => t.UserId).IsRequired();
            entity.HasIndex(t => t.UserId);
        });
    }
}
