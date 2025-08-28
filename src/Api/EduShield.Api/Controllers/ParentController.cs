using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EduShield.Core.Dtos;
using EduShield.Core.Interfaces;
using EduShield.Core.Enums;
using System.Security.Claims;

namespace EduShield.Api.Controllers;

/// <summary>
/// Controller for managing parent operations and parent portal functionality
/// </summary>
[ApiController]
[Route("api/v1/parents")]
[Authorize]
public class ParentController : ControllerBase
{
    private readonly IParentService _parentService;
    private readonly ILogger<ParentController> _logger;

    public ParentController(IParentService parentService, ILogger<ParentController> logger)
    {
        _parentService = parentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all parents (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IEnumerable<ParentResponse>>> GetAllParents()
    {
        try
        {
            var parents = await _parentService.GetAllAsync();
            return Ok(parents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all parents");
            return StatusCode(500, new { error = "An error occurred while retrieving parents." });
        }
    }

    /// <summary>
    /// Get parent by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ParentResponse>> GetParentById(Guid id)
    {
        try
        {
            var parent = await _parentService.GetByIdAsync(id);
            if (parent == null)
            {
                return NotFound(new { error = "Parent not found." });
            }

            // Check if current user can access this parent
            if (!CanAccessParent(id))
            {
                return Forbid();
            }

            return Ok(parent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parent with ID: {ParentId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the parent." });
        }
    }

    /// <summary>
    /// Get parent with children by ID (Parent Portal)
    /// </summary>
    [HttpGet("{id:guid}/with-children")]
    public async Task<ActionResult<ParentResponse>> GetParentWithChildren(Guid id)
    {
        try
        {
            var parent = await _parentService.GetWithChildrenByIdAsync(id);
            if (parent == null)
            {
                return NotFound(new { error = "Parent not found." });
            }

            // Check if current user can access this parent
            if (!CanAccessParent(id))
            {
                return Forbid();
            }

            return Ok(parent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parent with children, ID: {ParentId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the parent." });
        }
    }

    /// <summary>
    /// Get parent by email
    /// </summary>
    [HttpGet("by-email/{email}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ParentResponse>> GetParentByEmail(string email)
    {
        try
        {
            var parent = await _parentService.GetByEmailAsync(email);
            if (parent == null)
            {
                return NotFound(new { error = "Parent not found." });
            }

            return Ok(parent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parent by email: {Email}", email);
            return StatusCode(500, new { error = "An error occurred while retrieving the parent." });
        }
    }

    /// <summary>
    /// Get parent by user ID
    /// </summary>
    [HttpGet("by-user/{userId:guid}")]
    public async Task<ActionResult<ParentResponse>> GetParentByUserId(Guid userId)
    {
        try
        {
            var parent = await _parentService.GetByUserIdAsync(userId);
            if (parent == null)
            {
                return NotFound(new { error = "Parent not found." });
            }

            // Check if current user can access this parent
            if (!CanAccessParent(parent.Id))
            {
                return Forbid();
            }

            return Ok(parent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parent by user ID: {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while retrieving the parent." });
        }
    }

    /// <summary>
    /// Get parents by type
    /// </summary>
    [HttpGet("by-type/{parentType}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IEnumerable<ParentResponse>>> GetParentsByType(ParentType parentType)
    {
        try
        {
            var parents = await _parentService.GetByTypeAsync(parentType);
            return Ok(parents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parents by type: {ParentType}", parentType);
            return StatusCode(500, new { error = "An error occurred while retrieving parents." });
        }
    }

    /// <summary>
    /// Get parents by city
    /// </summary>
    [HttpGet("by-city/{city}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IEnumerable<ParentResponse>>> GetParentsByCity(string city)
    {
        try
        {
            var parents = await _parentService.GetByCityAsync(city);
            return Ok(parents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parents by city: {City}", city);
            return StatusCode(500, new { error = "An error occurred while retrieving parents." });
        }
    }

    /// <summary>
    /// Get parents by state
    /// </summary>
    [HttpGet("by-state/{state}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IEnumerable<ParentResponse>>> GetParentsByState(string state)
    {
        try
        {
            var parents = await _parentService.GetByStateAsync(state);
            return Ok(parents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parents by state: {State}", state);
            return StatusCode(500, new { error = "An error occurred while retrieving parents." });
        }
    }

    /// <summary>
    /// Search parents by name
    /// </summary>
    [HttpGet("search")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IEnumerable<ParentResponse>>> SearchParents([FromQuery] string q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(new { error = "Search query is required." });
            }

            var parents = await _parentService.SearchByNameAsync(q);
            return Ok(parents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching parents with query: {Query}", q);
            return StatusCode(500, new { error = "An error occurred while searching parents." });
        }
    }

    /// <summary>
    /// Get emergency contacts
    /// </summary>
    [HttpGet("emergency-contacts")]
    [Authorize(Policy = "AdminOrFaculty")]
    public async Task<ActionResult<IEnumerable<ParentResponse>>> GetEmergencyContacts()
    {
        try
        {
            var parents = await _parentService.GetEmergencyContactsAsync();
            return Ok(parents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving emergency contacts");
            return StatusCode(500, new { error = "An error occurred while retrieving emergency contacts." });
        }
    }

    /// <summary>
    /// Get parents authorized for pickup
    /// </summary>
    [HttpGet("authorized-for-pickup")]
    [Authorize(Policy = "AdminOrFaculty")]
    public async Task<ActionResult<IEnumerable<ParentResponse>>> GetAuthorizedForPickup()
    {
        try
        {
            var parents = await _parentService.GetAuthorizedForPickupAsync();
            return Ok(parents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parents authorized for pickup");
            return StatusCode(500, new { error = "An error occurred while retrieving parents." });
        }
    }

    /// <summary>
    /// Create new parent (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ParentResponse>> CreateParent([FromBody] CreateParentRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var parent = await _parentService.CreateAsync(request);
            return CreatedAtAction(nameof(GetParentById), new { id = parent.Id }, parent);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating parent");
            return StatusCode(500, new { error = "An error occurred while creating the parent." });
        }
    }

    /// <summary>
    /// Update existing parent
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ParentResponse>> UpdateParent(Guid id, [FromBody] UpdateParentRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if current user can access this parent
            if (!CanAccessParent(id))
            {
                return Forbid();
            }

            var parent = await _parentService.UpdateAsync(id, request);
            return Ok(parent);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating parent with ID: {ParentId}", id);
            return StatusCode(500, new { error = "An error occurred while updating the parent." });
        }
    }

    /// <summary>
    /// Delete parent (Admin only)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> DeleteParent(Guid id)
    {
        try
        {
            var result = await _parentService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new { error = "Parent not found." });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting parent with ID: {ParentId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the parent." });
        }
    }

    /// <summary>
    /// Add child to parent
    /// </summary>
    [HttpPost("{parentId:guid}/children/{childId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> AddChildToParent(Guid parentId, Guid childId)
    {
        try
        {
            var result = await _parentService.AddChildAsync(parentId, childId);
            if (!result)
            {
                return BadRequest(new { error = "Failed to add child to parent." });
            }

            return Ok(new { message = "Child successfully added to parent." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding child {ChildId} to parent {ParentId}", childId, parentId);
            return StatusCode(500, new { error = "An error occurred while adding child to parent." });
        }
    }

    /// <summary>
    /// Remove child from parent
    /// </summary>
    [HttpDelete("{parentId:guid}/children/{childId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> RemoveChildFromParent(Guid parentId, Guid childId)
    {
        try
        {
            var result = await _parentService.RemoveChildAsync(parentId, childId);
            if (!result)
            {
                return BadRequest(new { error = "Failed to remove child from parent." });
            }

            return Ok(new { message = "Child successfully removed from parent." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing child {ChildId} from parent {ParentId}", childId, parentId);
            return StatusCode(500, new { error = "An error occurred while removing child from parent." });
        }
    }

    /// <summary>
    /// Get parent statistics (Admin only)
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ParentStatistics>> GetParentStatistics()
    {
        try
        {
            var statistics = await _parentService.GetStatisticsAsync();
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parent statistics");
            return StatusCode(500, new { error = "An error occurred while retrieving parent statistics." });
        }
    }

    /// <summary>
    /// Get current user's parent profile (Parent Portal)
    /// </summary>
    [HttpGet("profile")]
    [Authorize(Policy = "ParentOnly")]
    public async Task<ActionResult<ParentResponse>> GetCurrentUserParentProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { error = "User ID not found in token." });
            }

            var parent = await _parentService.GetByUserIdAsync(userId.Value);
            if (parent == null)
            {
                return NotFound(new { error = "Parent profile not found for current user." });
            }

            return Ok(parent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user parent profile");
            return StatusCode(500, new { error = "An error occurred while retrieving the parent profile." });
        }
    }

    /// <summary>
    /// Update current user's parent profile (Parent Portal)
    /// </summary>
    [HttpPut("profile")]
    [Authorize(Policy = "ParentOnly")]
    public async Task<ActionResult<ParentResponse>> UpdateCurrentUserParentProfile([FromBody] UpdateParentRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { error = "User ID not found in token." });
            }

            var parent = await _parentService.GetByUserIdAsync(userId.Value);
            if (parent == null)
            {
                return NotFound(new { error = "Parent profile not found for current user." });
            }

            var updatedParent = await _parentService.UpdateAsync(parent.Id, request);
            return Ok(updatedParent);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating current user parent profile");
            return StatusCode(500, new { error = "An error occurred while updating the parent profile." });
        }
    }

    #region Helper Methods

    /// <summary>
    /// Check if current user can access the specified parent
    /// </summary>
    private bool CanAccessParent(Guid parentId)
    {
        var userRole = GetCurrentUserRole();
        var userId = GetCurrentUserId();

        // Admin and DevAuth can access all parents
        if (userRole == UserRole.Admin || userRole == UserRole.DevAuth)
        {
            return true;
        }

        // Parent can only access their own profile
        if (userRole == UserRole.Parent && userId.HasValue)
        {
            // This would need to be enhanced to check if the parent ID matches the current user's parent ID
            // For now, we'll allow access if the user is a parent
            return true;
        }

        return false;
    }

    /// <summary>
    /// Get current user ID from claims
    /// </summary>
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// Get current user role from claims
    /// </summary>
    private UserRole GetCurrentUserRole()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        if (Enum.TryParse<UserRole>(roleClaim, out var role))
        {
            return role;
        }
        return UserRole.Student; // Default fallback
    }

    #endregion
}

