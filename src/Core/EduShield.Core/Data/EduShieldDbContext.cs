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
    public DbSet<Parent> Parents { get; set; }
    public DbSet<StudentFaculty> StudentFaculties { get; set; }
    public DbSet<ParentStudent> ParentStudents { get; set; }
    public DbSet<StudentPerformance> StudentPerformances { get; set; }
    public DbSet<StudentFee> StudentFees { get; set; }

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

        // Configure ParentStudent many-to-many relationship
        modelBuilder.Entity<ParentStudent>(entity =>
        {
            entity.HasKey(ps => new { ps.ParentId, ps.StudentId });
            
            entity.Property(ps => ps.Relationship).IsRequired().HasMaxLength(50);
            entity.Property(ps => ps.Notes).HasMaxLength(500);
            entity.HasIndex(ps => ps.ParentId);
            entity.HasIndex(ps => ps.StudentId);
            entity.HasIndex(ps => new { ps.StudentId, ps.IsPrimaryContact });
            
            entity.HasOne(ps => ps.Parent)
                .WithMany(p => p.ParentStudents)
                .HasForeignKey(ps => ps.ParentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(ps => ps.Student)
                .WithMany(s => s.ParentStudents)
                .HasForeignKey(ps => ps.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure StudentPerformance entity
        modelBuilder.Entity<StudentPerformance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.StudentId, e.Subject, e.ExamType, e.ExamDate }).IsUnique();
            
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EncryptedScore).IsRequired();
            entity.Property(e => e.ExamTitle).HasMaxLength(200);
            entity.Property(e => e.Comments).HasMaxLength(500);
            
            // Relationships
            entity.HasOne(sp => sp.Student)
                .WithMany()
                .HasForeignKey(sp => sp.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Parent entity
        modelBuilder.Entity<Parent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.PhoneNumber);
            entity.HasIndex(e => e.City);
            entity.HasIndex(e => e.State);
            
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.AlternatePhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Occupation).HasMaxLength(100);
            entity.Property(e => e.Employer).HasMaxLength(100);
            entity.Property(e => e.WorkPhone).HasMaxLength(20);
            entity.Property(e => e.EmergencyContactName).HasMaxLength(100);
            entity.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
            entity.Property(e => e.EmergencyContactRelationship).HasMaxLength(50);
            entity.Property(e => e.ParentType).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            
            // Relationships
            entity.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);
                
            // One-to-many relationship with Student
            entity.HasMany(p => p.Children)
                .WithOne(s => s.Parent)
                .HasForeignKey(s => s.ParentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure StudentFee entity
        modelBuilder.Entity<StudentFee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.StudentId, e.FeeType, e.Term }).IsUnique();
            
            entity.Property(e => e.Term).IsRequired().HasMaxLength(20);
            entity.Property(e => e.EncryptedTotalAmount).IsRequired();
            entity.Property(e => e.EncryptedAmountPaid).IsRequired();
            entity.Property(e => e.EncryptedAmountDue).IsRequired();
            entity.Property(e => e.EncryptedFineAmount).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);
            
            // Relationships
            entity.HasOne(sf => sf.Student)
                .WithMany()
                .HasForeignKey(sf => sf.StudentId)
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
