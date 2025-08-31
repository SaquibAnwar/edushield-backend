using EduShield.Core.Entities;
using EduShield.Core.Interfaces;
using EduShield.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduShield.Core.Data;

/// <summary>
/// Repository implementation for Parent entity operations
/// </summary>
public class ParentRepository : IParentRepository
{
    private readonly EduShieldDbContext _context;

    public ParentRepository(EduShieldDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Parent>> GetAllAsync()
    {
        return await _context.Parents
            .Include(p => p.Children)
            .Include(p => p.User)
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    public async Task<Parent?> GetByIdAsync(Guid id)
    {
        return await _context.Parents
            .Include(p => p.Children)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Parent?> GetByEmailAsync(string email)
    {
        return await _context.Parents
            .Include(p => p.Children)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Email == email);
    }

    public async Task<Parent?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Parents
            .Include(p => p.Children)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);
    }

    public async Task<IEnumerable<Parent>> GetByTypeAsync(ParentType parentType)
    {
        return await _context.Parents
            .Include(p => p.Children)
            .Include(p => p.User)
            .Where(p => p.ParentType == parentType)
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Parent>> GetWithChildrenAsync()
    {
        return await _context.Parents
            .Include(p => p.Children)
            .Include(p => p.User)
            .Where(p => p.Children.Any())
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    public async Task<Parent?> GetWithChildrenByIdAsync(Guid id)
    {
        return await _context.Parents
            .Include(p => p.Children)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Parent>> GetByCityAsync(string city)
    {
        return await _context.Parents
            .Include(p => p.Children)
            .Include(p => p.User)
            .Where(p => p.City == city)
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Parent>> GetByStateAsync(string state)
    {
        return await _context.Parents
            .Include(p => p.Children)
            .Include(p => p.User)
            .Where(p => p.State == state)
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Parent>> SearchByNameAsync(string searchTerm)
    {
        var normalizedSearchTerm = searchTerm.ToLower();
        return await _context.Parents
            .Include(p => p.Children)
            .Include(p => p.User)
            .Where(p => p.FirstName.ToLower().Contains(normalizedSearchTerm) ||
                        p.LastName.ToLower().Contains(normalizedSearchTerm))
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Parent>> GetEmergencyContactsAsync()
    {
        return await _context.Parents
            .Include(p => p.Children)
            .Include(p => p.User)
            .Where(p => p.IsEmergencyContact && p.IsActive)
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Parent>> GetAuthorizedForPickupAsync()
    {
        return await _context.Parents
            .Include(p => p.Children)
            .Include(p => p.User)
            .Where(p => p.IsAuthorizedToPickup && p.IsActive)
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    public async Task<Parent> AddAsync(Parent parent)
    {
        _context.Parents.Add(parent);
        await _context.SaveChangesAsync();
        return parent;
    }

    public async Task<Parent> UpdateAsync(Parent parent)
    {
        _context.Parents.Update(parent);
        await _context.SaveChangesAsync();
        return parent;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var parent = await _context.Parents.FindAsync(id);
        if (parent == null) return false;

        // Soft delete - mark as inactive
        parent.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Parents.AnyAsync(p => p.Id == id);
    }

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null)
    {
        var query = _context.Parents.Where(p => p.Email == email);
        
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }

    public async Task<ParentStatistics> GetStatisticsAsync()
    {
        var parents = await _context.Parents
            .Include(p => p.Children)
            .ToListAsync();

        var statistics = new ParentStatistics
        {
            TotalParents = parents.Count,
            ActiveParents = parents.Count(p => p.IsActive),
            PrimaryParents = parents.Count(p => p.ParentType == ParentType.Primary),
            SecondaryParents = parents.Count(p => p.ParentType == ParentType.Secondary),
            Guardians = parents.Count(p => p.ParentType == ParentType.Guardian),
            EmergencyContacts = parents.Count(p => p.IsEmergencyContact),
            AuthorizedForPickup = parents.Count(p => p.IsAuthorizedToPickup),
            ParentsWithChildren = parents.Count(p => p.Children.Any()),
            AverageChildrenPerParent = parents.Any() ? (int)Math.Round(parents.Average(p => p.Children.Count)) : 0
        };

        // Group by state (all parents)
        statistics.ParentsByState = parents
            .Where(p => !string.IsNullOrEmpty(p.State))
            .GroupBy(p => p.State!)
            .ToDictionary(g => g.Key, g => g.Count());

        // Group by city (all parents)
        statistics.ParentsByCity = parents
            .Where(p => !string.IsNullOrEmpty(p.City))
            .GroupBy(p => p.City!)
            .ToDictionary(g => g.Key, g => g.Count());

        return statistics;
    }
}
