using EduShield.Core.Dtos;

namespace EduShield.Core.Services;

public interface IGoogleAuthService
{
    Task<GoogleUserInfo?> VerifyGoogleTokenAsync(string idToken);
}

public class GoogleUserInfo
{
    public string Sub { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Picture { get; set; }
    public bool EmailVerified { get; set; }
}
