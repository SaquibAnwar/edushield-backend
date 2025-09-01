using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EduShield.Core.Entities;
using EduShield.Core.Enums;
using EduShield.Core.Interfaces;
using EduShield.Core.Dtos;
using System.Security.Claims;

namespace EduShield.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, new { error = "Failed to retrieve users" });
        }
    }

    /// <summary>
    /// Get user by ID (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(500, new { error = "Failed to retrieve user" });
        }
    }

    /// <summary>
    /// Update user role (Admin only)
    /// </summary>
    [HttpPut("{id}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleRequest request)
    {
        try
        {
            var currentUserEmail = User.FindFirst("email")?.Value;
            if (string.IsNullOrEmpty(currentUserEmail))
            {
                currentUserEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            }
            if (string.IsNullOrEmpty(currentUserEmail))
            {
                currentUserEmail = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
            }
            
            var currentUserRole = User.FindFirst("role")?.Value;
            
            // Also check the standard role claim
            if (string.IsNullOrEmpty(currentUserRole))
            {
                currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            }

            _logger.LogInformation("User {Email} with role {Role} attempting to change user role", currentUserEmail, currentUserRole);

            if (currentUserRole != "Admin")
            {
                return StatusCode(403, new { error = "Only administrators can change user roles" });
            }

            var updatedUser = await _userService.UpdateUserRoleAsync(id, request.Role);
            if (updatedUser == null)
            {
                return NotFound(new { error = "User not found" });
            }

            _logger.LogInformation("User {UserId} role updated to {NewRole} by admin {AdminEmail}", 
                id, request.Role, currentUserEmail);

            return Ok(updatedUser);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role for user {UserId}", id);
            return StatusCode(500, new { error = "Failed to update user role" });
        }
    }

    /// <summary>
    /// Activate/Deactivate user (Admin only)
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusRequest request)
    {
        try
        {
            var currentUserEmail = User.FindFirst("email")?.Value;
            if (string.IsNullOrEmpty(currentUserEmail))
            {
                currentUserEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            }
            if (string.IsNullOrEmpty(currentUserEmail))
            {
                currentUserEmail = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
            }
            
            var currentUserRole = User.FindFirst("role")?.Value;
            
            // Also check the standard role claim
            if (string.IsNullOrEmpty(currentUserRole))
            {
                currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            }

            _logger.LogInformation("User {Email} with role {Role} attempting to change user status", currentUserEmail, currentUserRole);

            if (currentUserRole != "Admin")
            {
                return StatusCode(403, new { error = "Only administrators can change user status" });
            }

            var updatedUser = await _userService.UpdateUserStatusAsync(id, request.IsActive);
            if (updatedUser == null)
            {
                return NotFound(new { error = "User not found" });
            }

            _logger.LogInformation("User {UserId} status updated to {Status} by admin {AdminEmail}", 
                id, request.IsActive ? "Active" : "Inactive", currentUserEmail);

            return Ok(updatedUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user status for user {UserId}", id);
            return StatusCode(500, new { error = "Failed to update user status" });
        }
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("profile")]
    public async Task<ActionResult<UserDto>> GetCurrentUserProfile()
    {
        try
        {
            var userEmail = User.FindFirst("email")?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new { error = "User email not found in token" });
            }

            var user = await _userService.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return NotFound(new { error = "User profile not found" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user profile");
            return StatusCode(500, new { error = "Failed to retrieve user profile" });
        }
    }
}

/// <summary>
/// Request model for updating user role
/// </summary>
public class UpdateUserRoleRequest
{
    public UserRole Role { get; set; }
}

/// <summary>
/// Request model for updating user status
/// </summary>
public class UpdateUserStatusRequest
{
    public bool IsActive { get; set; }
}