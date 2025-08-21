using Microsoft.EntityFrameworkCore;
using EduShield.Core.Entities;

namespace EduShield.Core.Data;

public class EduShieldDbContext : DbContext
{
    public EduShieldDbContext(DbContextOptions<EduShieldDbContext> options) : base(options)
    {
    }

    public DbSet<Entities.User> Users { get; set; }

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
