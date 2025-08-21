using EduShield.Core.Dtos;

namespace EduShield.Core.Services;

/// <summary>
/// Service interface for Google OAuth authentication
/// </summary>
public interface IGoogleAuthService
{
    /// <summary>
    /// Verifies a Google ID token and returns user information
    /// </summary>
    /// <param name="idToken">The Google ID token to verify</param>
    /// <returns>Google user information if token is valid, null otherwise</returns>
    Task<GoogleUserInfo?> VerifyGoogleTokenAsync(string idToken);
}

/// <summary>
/// Represents user information received from Google OAuth
/// </summary>
public class GoogleUserInfo
{
    /// <summary>
    /// Google's unique identifier for the user
    /// </summary>
    /// <example>110169484474386276334</example>
    public string Sub { get; set; } = string.Empty;

    /// <summary>
    /// User's email address from Google account
    /// </summary>
    /// <example>iamsaquibanwar@gmail.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's full name from Google account
    /// </summary>
    /// <example>Saquib Anwar</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL to the user's Google profile picture
    /// </summary>
    /// <example>https://lh3.googleusercontent.com/a/ACg8ocJ...</example>
    public string? Picture { get; set; }

    /// <summary>
    /// Indicates whether the user's email has been verified by Google
    /// </summary>
    /// <example>true</example>
    public bool EmailVerified { get; set; }
}
