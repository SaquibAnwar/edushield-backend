using Microsoft.AspNetCore.Mvc;
using EduShield.Core.Dtos;
using EduShield.Core.Services;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace EduShield.Api.Controllers;

/// <summary>
/// Authentication controller providing Google OAuth, Dev Authentication, and token management endpoints
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), 400)]
[ProducesResponseType(typeof(ProblemDetails), 500)]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authenticate user using Google OAuth ID token
    /// </summary>
    /// <remarks>
    /// This endpoint validates a Google ID token and returns a JWT token for the authenticated user.
    /// The user must have a valid Google account and be registered in the system.
    /// 
    /// **Flow:**
    /// 1. User authenticates with Google (frontend)
    /// 2. Google returns ID token
    /// 3. Frontend sends ID token to this endpoint
    /// 4. Backend validates token and returns JWT
    /// 
    /// **Sample Request:**
    /// ```json
    /// {
    ///   "idToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9..."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Google authentication request containing the ID token</param>
    /// <returns>Authentication result with JWT token and user information</returns>
    /// <response code="200">Authentication successful. Returns JWT token and user details.</response>
    /// <response code="400">Invalid request format or missing ID token.</response>
    /// <response code="401">Authentication failed. Invalid or expired Google token.</response>
    /// <response code="500">Internal server error during authentication.</response>
    [HttpPost("google")]
    [ProducesResponseType(typeof(AuthResult), 200)]
    [ProducesResponseType(typeof(AuthResult), 401)]
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

    /// <summary>
    /// Authenticate user using development authentication (bypass for testing)
    /// </summary>
    /// <remarks>
    /// This endpoint provides development authentication bypass for testing purposes.
    /// It allows developers to authenticate with predefined user accounts without going through OAuth.
    /// 
    /// **Available Test Accounts:**
    /// - **Admin**: `iamsaquibanwar@gmail.com` (Role: Admin)
    /// - **Student**: `saquibanwar01@gmail.com` (Role: Student)
    /// - **Faculty**: `saquibedu@gmail.com` (Role: Faculty)
    /// - **Parent**: `kirakryto9ite@gmail.com` (Role: Parent)
    /// - **DevAuth**: `techtonicwave.business@gmail.com` (Role: DevAuth)
    /// 
    /// **Sample Request:**
    /// ```json
    /// {
    ///   "email": "iamsaquibanwar@gmail.com"
    /// }
    /// ```
    /// 
    /// **Note:** This endpoint is only available when `EnableDevAuth` is set to `true` in configuration.
    /// </remarks>
    /// <param name="request">Development authentication request containing the email</param>
    /// <returns>Authentication result with JWT token and user information</returns>
    /// <response code="200">Authentication successful. Returns JWT token and user details.</response>
    /// <response code="400">Invalid request format or missing email.</response>
    /// <response code="401">Authentication failed. User not found or dev auth disabled.</response>
    /// <response code="500">Internal server error during authentication.</response>
    [HttpPost("dev")]
    [ProducesResponseType(typeof(AuthResult), 200)]
    [ProducesResponseType(typeof(AuthResult), 401)]
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

    /// <summary>
    /// Refresh an expired JWT token using a valid refresh token
    /// </summary>
    /// <remarks>
    /// This endpoint allows users to obtain a new JWT token using their refresh token.
    /// This is useful when the JWT token expires but the refresh token is still valid.
    /// 
    /// **How it works:**
    /// 1. User's JWT token expires
    /// 2. User sends refresh token to this endpoint
    /// 3. Backend validates refresh token
    /// 4. Backend returns new JWT token and refresh token
    /// 
    /// **Sample Request:**
    /// ```json
    /// {
    ///   "refreshToken": "sryOyRBjBljN+QFYihPuJYUFcWdub0pMFEJNtmBdmoEvQmMFpYekuQhCWM3x2L88YAd4Xd59HfmMe3c63o156A=="
    /// }
    /// ```
    /// 
    /// **Note:** Currently returns "Please re-authenticate" as the refresh token system is not fully implemented.
    /// </remarks>
    /// <param name="request">Refresh token request containing the refresh token</param>
    /// <returns>Authentication result with new JWT token and refresh token</returns>
    /// <response code="200">Token refresh successful. Returns new JWT token and refresh token.</response>
    /// <response code="400">Invalid request format or missing refresh token.</response>
    /// <response code="401">Token refresh failed. Invalid or expired refresh token.</response>
    /// <response code="500">Internal server error during token refresh.</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResult), 200)]
    [ProducesResponseType(typeof(AuthResult), 401)]
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

    /// <summary>
    /// Revoke a refresh token to invalidate it
    /// </summary>
    /// <remarks>
    /// This endpoint allows users to revoke their refresh token, making it invalid for future use.
    /// This is useful for security purposes, such as when a user logs out or suspects their token has been compromised.
    /// 
    /// **How it works:**
    /// 1. User sends refresh token to this endpoint
    /// 2. Backend marks the refresh token as revoked
    /// 3. The refresh token can no longer be used to obtain new JWT tokens
    /// 
    /// **Sample Request:**
    /// ```json
    /// {
    ///   "refreshToken": "sryOyRBjBljN+QFYihPuJYUFcWdub0pMFEJNtmBdmoEvQmMFpYekuQhCWM3x2L88YAd4Xd59HfmMe3c63o156A=="
    /// }
    /// ```
    /// 
    /// **Note:** Currently always returns success as the revocation system is not fully implemented.
    /// </remarks>
    /// <param name="request">Refresh token request containing the refresh token to revoke</param>
    /// <returns>Success message indicating the token was revoked</returns>
    /// <response code="200">Token revoked successfully.</response>
    /// <response code="400">Invalid request format or missing refresh token.</response>
    /// <response code="500">Internal server error during token revocation.</response>
    [HttpPost("revoke")]
    [ProducesResponseType(typeof(object), 200)]
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
