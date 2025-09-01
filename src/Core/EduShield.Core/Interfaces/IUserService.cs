using EduShield.Core.Entities;
using EduShield.Core.Enums;
using EduShield.Core.Dtos;

namespace EduShield.Core.Interfaces;

/// <summary>
/// Service interface for user management operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Get all users
    /// </summary>
    Task<IEnumerable<UserDto>> GetAllUsersAsync();

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<UserDto?> GetUserByIdAsync(Guid id);

    /// <summary>
    /// Get user by email
    /// </summary>
    Task<UserDto?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Update user role
    /// </summary>
    Task<UserDto?> UpdateUserRoleAsync(Guid id, UserRole newRole);

    /// <summary>
    /// Update user status (active/inactive)
    /// </summary>
    Task<UserDto?> UpdateUserStatusAsync(Guid id, bool isActive);

    /// <summary>
    /// Create or update user from authentication
    /// </summary>
    Task<User> CreateOrUpdateUserAsync(string email, string name, string? googleId = null, string? profilePictureUrl = null);
}