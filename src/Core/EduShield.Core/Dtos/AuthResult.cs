using EduShield.Core.Enums;

namespace EduShield.Core.Dtos;

/// <summary>
/// Represents the result of an authentication operation
/// </summary>
public class AuthResult
{
    /// <summary>
    /// Indicates whether the authentication was successful
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// The JWT token issued upon successful authentication
    /// </summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    public string? Token { get; set; }

    /// <summary>
    /// The refresh token for obtaining new JWT tokens
    /// </summary>
    /// <example>sryOyRBjBljN+QFYihPuJYUFcWdub0pMFEJNtmBdmoEvQmMFpYekuQhCWM3x2L88YAd4Xd59HfmMe3c63o156A==</example>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// The expiration date and time of the JWT token
    /// </summary>
    /// <example>2025-08-21T16:51:12.333415Z</example>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// The authenticated user's information
    /// </summary>
    public UserDto? User { get; set; }

    /// <summary>
    /// Error message if authentication failed
    /// </summary>
    /// <example>Invalid Google token</example>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Request model for Google OAuth authentication
/// </summary>
public class GoogleAuthRequest
{
    /// <summary>
    /// The Google ID token received from Google OAuth flow
    /// </summary>
    /// <example>eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    public string IdToken { get; set; } = string.Empty;
}

/// <summary>
/// Request model for refreshing JWT tokens
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// The refresh token used to obtain a new JWT token
    /// </summary>
    /// <example>sryOyRBjBljN+QFYihPuJYUFcWdub0pMFEJNtmBdmoEvQmMFpYekuQhCWM3x2L88YAd4Xd59HfmMe3c63o156A==</example>
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Request model for development authentication (bypass for testing)
/// </summary>
public class DevAuthRequest
{
    /// <summary>
    /// The email address of the user to authenticate
    /// </summary>
    /// <example>iamsaquibanwar@gmail.com</example>
    public string Email { get; set; } = string.Empty;
}
