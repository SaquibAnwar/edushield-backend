using EduShield.Core.Enums;

namespace EduShield.Core.Dtos;

/// <summary>
/// Data transfer object for user information
/// </summary>
public class UserDto
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    /// <example>443abd4f-9e56-4adc-9eb7-7a0e2522dd2b</example>
    public Guid Id { get; set; }

    /// <summary>
    /// User's email address (used for authentication)
    /// </summary>
    /// <example>iamsaquibanwar@gmail.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's display name
    /// </summary>
    /// <example>Saquib Admin</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL to the user's profile picture (optional)
    /// </summary>
    /// <example>https://lh3.googleusercontent.com/a/ACg8ocJ...</example>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// User's role in the system
    /// </summary>
    /// <example>Admin</example>
    public UserRole Role { get; set; }

    /// <summary>
    /// Indicates whether the user account is active
    /// </summary>
    /// <example>true</example>
    public bool IsActive { get; set; }

    /// <summary>
    /// Date and time when the user account was created
    /// </summary>
    /// <example>2025-08-21T15:36:32.965405Z</example>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the user account was last updated
    /// </summary>
    /// <example>2025-08-21T15:36:32.965405Z</example>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Date and time when the user last logged in (optional)
    /// </summary>
    /// <example>2025-08-21T16:35:29.726832Z</example>
    public DateTime? LastLoginAt { get; set; }
}
