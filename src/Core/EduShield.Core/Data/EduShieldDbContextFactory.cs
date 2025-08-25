using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EduShield.Core.Data;

/// <summary>
/// Design-time factory for Entity Framework migrations
/// </summary>
public class EduShieldDbContextFactory : IDesignTimeDbContextFactory<EduShieldDbContext>
{
    public EduShieldDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EduShieldDbContext>();
        
        // Use a connection string suitable for design-time operations
        // This will be overridden at runtime with the actual connection string
        optionsBuilder.UseNpgsql("Host=localhost;Database=edushield_backend;Username=postgres;Password=postgres123;Port=5433");
        
        return new EduShieldDbContext(optionsBuilder.Options);
    }
}
