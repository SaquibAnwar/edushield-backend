using Microsoft.EntityFrameworkCore;
using EduShield.Core.Entities;

namespace EduShield.Core.Data;

public class EduShieldDbContext : DbContext
{
    public EduShieldDbContext(DbContextOptions<EduShieldDbContext> options) : base(options)
    {
    }

    public DbSet<Entities.User> Users { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Faculty> Faculty { get; set; }
    public DbSet<StudentFaculty> StudentFaculties { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure User entity
        modelBuilder.Entity<Entities.User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.GoogleId).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Role).IsRequired();
        });

        // Configure Student entity
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.RollNumber).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
            entity.Property(e => e.RollNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Grade).HasMaxLength(20);
            entity.Property(e => e.Section).HasMaxLength(10);
            
            // Relationships
            entity.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(s => s.Parent)
                .WithMany()
                .HasForeignKey(s => s.ParentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Faculty entity
        modelBuilder.Entity<Faculty>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.EmployeeId).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Department).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EmployeeId).HasMaxLength(50);
            
            // Relationships
            entity.HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure StudentFaculty many-to-many relationship
        modelBuilder.Entity<StudentFaculty>(entity =>
        {
            entity.HasKey(sf => new { sf.StudentId, sf.FacultyId });
            
            entity.HasOne(sf => sf.Student)
                .WithMany(s => s.StudentFaculties)
                .HasForeignKey(sf => sf.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(sf => sf.Faculty)
                .WithMany(f => f.StudentFaculties)
                .HasForeignKey(sf => sf.FacultyId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override int SaveChanges()
    {
        StampAudit();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAudit();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void StampAudit()
    {
        var entities = ChangeTracker.Entries<AuditableEntity>();
        
        foreach (var entity in entities)
        {
            if (entity.State == EntityState.Added)
            {
                entity.Entity.CreatedAt = DateTime.UtcNow;
                entity.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entity.State == EntityState.Modified)
            {
                entity.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
