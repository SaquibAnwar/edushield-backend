using EduShield.Core.Entities;
using EduShield.Core.Enums;
using EduShield.Core.Interfaces;
using EduShield.Core.Dtos;
using Microsoft.Extensions.Logging;

namespace EduShield.Core.Services;

/// <summary>
/// Service for user management operations
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            throw;
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user != null ? MapToDto(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            throw;
        }
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user != null ? MapToDto(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by email {Email}", email);
            throw;
        }
    }

    public async Task<UserDto?> UpdateUserRoleAsync(Guid id, UserRole newRole)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            // Validate role change
            if (!Enum.IsDefined(typeof(UserRole), newRole))
            {
                throw new ArgumentException($"Invalid role: {newRole}");
            }

            // Prevent changing DevAuth role in production
            if (user.Role == UserRole.DevAuth || newRole == UserRole.DevAuth)
            {
                throw new ArgumentException("DevAuth role cannot be changed through this endpoint");
            }

            var oldRole = user.Role;
            user.Role = newRole;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("User {UserId} role changed from {OldRole} to {NewRole}", 
                id, oldRole, newRole);

            return MapToDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role for user {UserId}", id);
            throw;
        }
    }

    public async Task<UserDto?> UpdateUserStatusAsync(Guid id, bool isActive)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("User {UserId} status changed to {Status}", 
                id, isActive ? "Active" : "Inactive");

            return MapToDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user status for user {UserId}", id);
            throw;
        }
    }

    public async Task<User> CreateOrUpdateUserAsync(string email, string name, string? googleId = null, string? profilePictureUrl = null)
    {
        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(email);
            
            if (existingUser != null)
            {
                // Update existing user
                existingUser.Name = name;
                if (!string.IsNullOrEmpty(googleId))
                    existingUser.GoogleId = googleId;
                if (!string.IsNullOrEmpty(profilePictureUrl))
                    existingUser.ProfilePictureUrl = profilePictureUrl;
                existingUser.LastLoginAt = DateTime.UtcNow;
                existingUser.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(existingUser);
                return existingUser;
            }
            else
            {
                // Create new user with default Student role
                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    Name = name,
                    GoogleId = googleId,
                    ProfilePictureUrl = profilePictureUrl,
                    Role = UserRole.Student, // Default role for new users
                    IsActive = true,
                    LastLoginAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _userRepository.CreateAsync(newUser);
                
                _logger.LogInformation("New user created with email {Email} and default role {Role}", 
                    email, UserRole.Student);
                
                return newUser;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating or updating user with email {Email}", email);
            throw;
        }
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}