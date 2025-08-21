using EduShield.Core.Configuration;
using EduShield.Core.Dtos;
using EduShield.Core.Enums;
using EduShield.Core.Interfaces;
using EduShield.Core.Security;
using Microsoft.Extensions.Options;

namespace EduShield.Core.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IJwtService _jwtService;
    private readonly AuthenticationConfiguration _authConfig;

    public AuthService(
        IUserRepository userRepository,
        IGoogleAuthService googleAuthService,
        IJwtService jwtService,
        IOptions<AuthenticationConfiguration> authConfig)
    {
        _userRepository = userRepository;
        _googleAuthService = googleAuthService;
        _jwtService = jwtService;
        _authConfig = authConfig.Value;
    }

    public async Task<AuthResult> AuthenticateWithGoogleAsync(string idToken)
    {
        try
        {
            var googleUser = await _googleAuthService.VerifyGoogleTokenAsync(idToken);
            if (googleUser == null)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Invalid Google token"
                };
            }

            var user = await _userRepository.GetByEmailAsync(googleUser.Email);
            if (user == null)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "User not found. Please contact administrator for access."
                };
            }

            if (!user.IsActive)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "User account is deactivated"
                };
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var userDto = MapToUserDto(user);
            var token = _jwtService.GenerateToken(userDto);
            var refreshToken = _jwtService.GenerateRefreshToken();

            return new AuthResult
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_authConfig.Jwt.ExpirationMinutes),
                User = userDto
            };
        }
        catch (Exception ex)
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Authentication failed: " + ex.Message
            };
        }
    }

    public async Task<AuthResult> AuthenticateWithDevAuthAsync(string email)
    {
        if (!_authConfig.EnableDevAuth)
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Dev authentication is disabled"
            };
        }

        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "User not found"
                };
            }

            var userDto = MapToUserDto(user);
            var token = _jwtService.GenerateToken(userDto);
            var refreshToken = _jwtService.GenerateRefreshToken();

            return new AuthResult
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_authConfig.Jwt.ExpirationMinutes),
                User = userDto
            };
        }
        catch (Exception ex)
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Dev authentication failed: " + ex.Message
            };
        }
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        if (!_jwtService.ValidateRefreshToken(refreshToken))
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Invalid refresh token"
            };
        }

        // In production, you'd validate the refresh token against the database
        // For now, we'll return an error suggesting to re-authenticate
        return new AuthResult
        {
            Success = false,
            ErrorMessage = "Please re-authenticate"
        };
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        // In production, you'd invalidate the refresh token in the database
        return true;
    }

    private static UserDto MapToUserDto(Entities.User user)
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
