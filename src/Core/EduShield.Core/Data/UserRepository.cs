using EduShield.Core.Entities;
using EduShield.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EduShield.Core.Data;

public class UserRepository : IUserRepository
{
    private readonly EduShieldDbContext _context;

    public UserRepository(EduShieldDbContext context)
    {
        _context = context;
    }

    public async Task<Entities.User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Entities.User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<Entities.User?> GetByGoogleIdAsync(string googleId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.GoogleId == googleId);
    }

    public async Task<IEnumerable<Entities.User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<Entities.User> CreateAsync(Entities.User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<Entities.User> UpdateAsync(Entities.User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }
}
