using EduShield.Core.Dtos;

namespace EduShield.Core.Services;

public interface IAuthService
{
    Task<AuthResult> AuthenticateWithGoogleAsync(string idToken);
    Task<AuthResult> AuthenticateWithDevAuthAsync(string email);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
}
