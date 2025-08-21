using EduShield.Core.Entities;

namespace EduShield.Core.Interfaces;

public interface IUserRepository
{
    Task<Entities.User?> GetByIdAsync(Guid id);
    Task<Entities.User?> GetByEmailAsync(string email);
    Task<Entities.User?> GetByGoogleIdAsync(string googleId);
    Task<IEnumerable<Entities.User>> GetAllAsync();
    Task<Entities.User> CreateAsync(Entities.User user);
    Task<Entities.User> UpdateAsync(Entities.User user);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(string email);
}
