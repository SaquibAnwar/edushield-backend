using EduShield.Core.Enums;

namespace EduShield.Core.Dtos;

public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public UserDto? User { get; set; }
    public string? ErrorMessage { get; set; }
}

public class GoogleAuthRequest
{
    public string IdToken { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class DevAuthRequest
{
    public string Email { get; set; } = string.Empty;
}
