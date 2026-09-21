using Microsoft.EntityFrameworkCore;
using WorkforceApi.Models;

namespace WorkforceApi.Data;

/// <summary>EF Core unit of work for the workforce database. One instance is created per HTTP request.</summary>
public class WorkforceDbContext : DbContext
{
    public WorkforceDbContext(DbContextOptions<WorkforceDbContext> options) : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Shift> Shifts => Set<Shift>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
            entity.Property(d => d.Description).HasMaxLength(500);
            entity.HasIndex(d => d.Name).IsUnique();
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(254);
            entity.Property(e => e.JobTitle).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Salary).HasPrecision(12, 2);
            entity.HasIndex(e => e.Email).IsUnique();

            // A department that still has employees can't be deleted; the API reports a conflict.
            entity.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Title).IsRequired().HasMaxLength(200);
            entity.Property(s => s.Notes).HasMaxLength(1000);
            entity.HasIndex(s => s.Start);

            entity.HasOne(s => s.Employee)
                .WithMany(e => e.Shifts)
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
