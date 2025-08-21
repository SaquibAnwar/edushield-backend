using Microsoft.AspNetCore.Mvc;
using EduShield.Core.Dtos;
using EduShield.Core.Services;

namespace EduShield.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("google")]
    public async Task<ActionResult<AuthResult>> AuthenticateWithGoogle([FromBody] GoogleAuthRequest request)
    {
        if (string.IsNullOrEmpty(request.IdToken))
        {
            return BadRequest(new { error = "ID token is required" });
        }

        var result = await _authService.AuthenticateWithGoogleAsync(request.IdToken);
        
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    [HttpPost("dev")]
    public async Task<ActionResult<AuthResult>> AuthenticateWithDevAuth([FromBody] DevAuthRequest request)
    {
        if (string.IsNullOrEmpty(request.Email))
        {
            return BadRequest(new { error = "Email is required" });
        }

        var result = await _authService.AuthenticateWithDevAuthAsync(request.Email);
        
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResult>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return BadRequest(new { error = "Refresh token is required" });
        }

        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    [HttpPost("revoke")]
    public async Task<ActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return BadRequest(new { error = "Refresh token is required" });
        }

        var success = await _authService.RevokeTokenAsync(request.RefreshToken);
        
        if (success)
        {
            return Ok(new { message = "Token revoked successfully" });
        }

        return BadRequest(new { error = "Failed to revoke token" });
    }
}
