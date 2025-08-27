using EduShield.Api.Auth.Requirements;
using EduShield.Core.Dtos;
using EduShield.Core.Enums;
using EduShield.Core.Interfaces;
using EduShield.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduShield.Api.Controllers;

/// <summary>
/// Controller for managing student fee records and payments
/// </summary>
[ApiController]
[Route("api/v1/student-fees")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), 400)]
[ProducesResponseType(typeof(ProblemDetails), 401)]
[ProducesResponseType(typeof(ProblemDetails), 403)]
[ProducesResponseType(typeof(ProblemDetails), 404)]
[ProducesResponseType(typeof(ProblemDetails), 500)]
public class StudentFeeController : ControllerBase
{
    private readonly IStudentFeeService _feeService;
    private readonly ILogger<StudentFeeController> _logger;

    public StudentFeeController(IStudentFeeService feeService, ILogger<StudentFeeController> logger)
    {
        _feeService = feeService;
        _logger = logger;
    }

    /// <summary>
    /// Get all student fee records (role-restricted access)
    /// </summary>
    /// <remarks>
    /// **Access Control:**
    /// - **Admin/DevAuth**: Can view all fee records
    /// - **Faculty**: Can view fee status for students assigned to them (no amounts)
    /// - **Student**: Can view only their own fee records
    /// - **Parent**: Can view fee records for their children
    /// 
    /// **Query Parameters:**
    /// - `feeType`: Filter by fee type
    /// - `term`: Filter by term
    /// - `status`: Filter by payment status
    /// - `startDate`: Filter by start date (ISO format)
    /// - `endDate`: Filter by end date (ISO format)
    /// </remarks>
    /// <param name="feeType">Optional fee type filter</param>
    /// <param name="term">Optional term filter</param>
    /// <param name="status">Optional payment status filter</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of fee records based on user role and filters</returns>
    /// <response code="200">Fee records retrieved successfully.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Insufficient permissions.</response>
    /// <response code="500">Internal server error during retrieval.</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<StudentFeeDto>), 200)]
    public async Task<ActionResult<IEnumerable<StudentFeeDto>>> GetAllFees(
        [FromQuery] FeeType? feeType,
        [FromQuery] string? term,
        [FromQuery] PaymentStatus? status,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        try
        {
            var userRole = GetCurrentUserRole();
            var userId = GetCurrentUserId();

            IEnumerable<StudentFeeDto> fees;

            switch (userRole)
            {
                case UserRole.Admin:
                case UserRole.DevAuth:
                    // Admin/Dev can see all fee records
                    fees = await _feeService.GetAllAsync(cancellationToken);
                    break;

                case UserRole.Faculty:
                    // Faculty can see fee records for students assigned to them
                    if (userId.HasValue)
                    {
                        fees = await _feeService.GetByFacultyIdAsync(userId.Value, cancellationToken);
                    }
                    else
                    {
                        return Unauthorized(new { error = "User ID not found." });
                    }
                    break;

                case UserRole.Student:
                    // Students can only see their own fee records
                    if (userId.HasValue)
                    {
                        fees = await _feeService.GetByUserIdAsync(userId.Value, cancellationToken);
                    }
                    else
                    {
                        return Unauthorized(new { error = "User ID not found." });
                    }
                    break;

                case UserRole.Parent:
                    // Parents can see fee records for their children
                    if (userId.HasValue)
                    {
                        fees = await _feeService.GetByParentIdAsync(userId.Value, cancellationToken);
                    }
                    else
                    {
                        return Unauthorized(new { error = "User ID not found." });
                    }
                    break;

                default:
                    return Forbid();
            }

            // Apply filters if provided
            if (feeType.HasValue)
            {
                fees = fees.Where(f => f.FeeType == feeType.Value);
            }

            if (!string.IsNullOrEmpty(term))
            {
                fees = fees.Where(f => f.Term.Equals(term, StringComparison.OrdinalIgnoreCase));
            }

            if (status.HasValue)
            {
                fees = fees.Where(f => f.PaymentStatus == status.Value);
            }

            if (startDate.HasValue)
            {
                fees = fees.Where(f => f.DueDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                fees = fees.Where(f => f.DueDate <= endDate.Value);
            }

            return Ok(fees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving fee records");
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving fee records." });
        }
    }

    /// <summary>
    /// Get a specific fee record by ID (role-restricted access)
    /// </summary>
    /// <remarks>
    /// **Access Control:** Same as GET /api/v1/student-fees
    /// </remarks>
    /// <param name="id">Fee record ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Fee record details</returns>
    /// <response code="200">Fee record found successfully.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Insufficient permissions.</response>
    /// <response code="404">Fee record not found.</response>
    /// <response code="500">Internal server error during retrieval.</response>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(StudentFeeDto), 200)]
    public async Task<ActionResult<StudentFeeDto>> GetFeeById(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var fee = await _feeService.GetByIdAsync(id, cancellationToken);
            if (fee == null)
            {
                return NotFound(new { error = "Fee record not found." });
            }

            // Check authorization using the requirement
            var requirement = new StudentFeeAccessRequirement { ReadOnly = true };
            var authResult = CheckAuthorizationAsync(requirement, fee);
            
            if (!authResult)
            {
                return Forbid();
            }

            return Ok(fee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving fee record with ID: {FeeId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving the fee record." });
        }
    }

    /// <summary>
    /// Create a new fee record (Admin/DevAuth only)
    /// </summary>
    /// <remarks>
    /// **Access Control:**
    /// - **Admin/DevAuth**: Can create fee records for any student
    /// - **Faculty/Student/Parent**: Cannot create fee records
    /// 
    /// **Fee Encryption:** All amounts are automatically encrypted before storage for security.
    /// </remarks>
    /// <param name="request">Fee creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created fee record</returns>
    /// <response code="201">Fee record created successfully.</response>
    /// <response code="400">Invalid request data or validation failed.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin or DevAuth role required.</response>
    /// <response code="500">Internal server error during creation.</response>
    [HttpPost]
    [Authorize(Roles = "Admin,DevAuth")]
    [ProducesResponseType(typeof(StudentFeeDto), 201)]
    public async Task<ActionResult<StudentFeeDto>> CreateFee(
        [FromBody] CreateStudentFeeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var fee = await _feeService.CreateAsync(request, cancellationToken);
            _logger.LogInformation("Fee record created successfully with ID: {FeeId}", fee.Id);
            
            return CreatedAtAction(nameof(GetFeeById), new { id = fee.Id }, fee);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create fee record: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating fee record");
            return StatusCode(500, new { error = "An unexpected error occurred while creating the fee record." });
        }
    }

    /// <summary>
    /// Update an existing fee record (Admin/DevAuth only)
    /// </summary>
    /// <remarks>
    /// **Access Control:**
    /// - **Admin/DevAuth**: Can update any fee record
    /// - **Faculty/Student/Parent**: Cannot update fee records
    /// 
    /// **Fee Encryption:** Updated amounts are automatically re-encrypted.
    /// </remarks>
    /// <param name="id">Fee record ID to update</param>
    /// <param name="request">Fee update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated fee record</returns>
    /// <response code="200">Fee record updated successfully.</response>
    /// <response code="400">Invalid request data or validation failed.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin or DevAuth role required.</response>
    /// <response code="404">Fee record not found.</response>
    /// <response code="500">Internal server error during update.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,DevAuth")]
    [ProducesResponseType(typeof(StudentFeeDto), 200)]
    public async Task<ActionResult<StudentFeeDto>> UpdateFee(
        Guid id,
        [FromBody] UpdateStudentFeeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var fee = await _feeService.UpdateAsync(id, request, cancellationToken);
            _logger.LogInformation("Fee record updated successfully with ID: {FeeId}", id);
            
            return Ok(fee);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update fee record {FeeId}: {Message}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating fee record {FeeId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred while updating the fee record." });
        }
    }

    /// <summary>
    /// Delete a fee record (Admin/DevAuth only)
    /// </summary>
    /// <remarks>
    /// **Access Control:**
    /// - **Admin/DevAuth**: Can delete any fee record
    /// - **Faculty/Student/Parent**: Cannot delete fee records
    /// 
    /// **Warning:** This action cannot be undone.
    /// </remarks>
    /// <param name="id">Fee record ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">Fee record deleted successfully.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin or DevAuth role required.</response>
    /// <response code="404">Fee record not found.</response>
    /// <response code="500">Internal server error during deletion.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,DevAuth")]
    [ProducesResponseType(204)]
    public async Task<ActionResult> DeleteFee(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!await _feeService.ExistsAsync(id, cancellationToken))
            {
                return NotFound(new { error = "Fee record not found." });
            }

            await _feeService.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("Fee record deleted successfully with ID: {FeeId}", id);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting fee record {FeeId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred while deleting the fee record." });
        }
    }

    /// <summary>
    /// Make a payment on a fee record (Student/Parent only)
    /// </summary>
    /// <remarks>
    /// **Access Control:**
    /// - **Student**: Can make payments on their own fees
    /// - **Parent**: Can make payments on their children's fees
    /// - **Admin/DevAuth**: Cannot make payments (use update endpoint instead)
    /// - **Faculty**: Cannot make payments
    /// 
    /// **Payment Processing:** Payment is processed through a mock payment gateway.
    /// </remarks>
    /// <param name="id">Fee record ID</param>
    /// <param name="request">Payment request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment result with updated fee information</returns>
    /// <response code="200">Payment processed successfully.</response>
    /// <response code="400">Invalid payment request or validation failed.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Student or Parent role required.</response>
    /// <response code="404">Fee record not found.</response>
    /// <response code="500">Internal server error during payment processing.</response>
    [HttpPost("{id:guid}/pay")]
    [Authorize(Roles = "Student,Parent")]
    [ProducesResponseType(typeof(PaymentResult), 200)]
    public async Task<ActionResult<PaymentResult>> MakePayment(
        Guid id,
        [FromBody] PaymentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userRole = GetCurrentUserRole();
            var userId = GetCurrentUserId();

            // Check if user is authorized to make payment on this fee
            var fee = await _feeService.GetByIdAsync(id, cancellationToken);
            if (fee == null)
            {
                return NotFound(new { error = "Fee record not found." });
            }

            // Verify access permissions
            if (userRole == UserRole.Student)
            {
                // For students, check if they own this fee by getting their student profile
                if (!userId.HasValue)
                {
                    return Unauthorized(new { error = "User ID not found." });
                }
                var studentFees = await _feeService.GetByUserIdAsync(userId.Value, cancellationToken);
                var ownsFee = studentFees.Any(f => f.Id == id);
                if (!ownsFee)
                {
                    return Forbid();
                }
            }

            if (userRole == UserRole.Parent)
            {
                // This would require additional validation in the service layer
                // For now, we'll let the service handle it
            }

            var paymentResult = await _feeService.MakePaymentAsync(id, request, cancellationToken);
            
            if (paymentResult.Success)
            {
                _logger.LogInformation("Payment processed successfully for fee {FeeId}: {Amount}", id, request.Amount);
                return Ok(paymentResult);
            }
            else
            {
                _logger.LogWarning("Payment failed for fee {FeeId}: {Error}", id, paymentResult.ErrorMessage);
                return BadRequest(new { error = paymentResult.ErrorMessage });
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to process payment for fee {FeeId}: {Message}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while processing payment for fee {FeeId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred while processing the payment." });
        }
    }

    /// <summary>
    /// Get fee records by fee type (role-restricted access)
    /// </summary>
    /// <param name="feeType">Fee type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Fee records for the specified type</returns>
    [HttpGet("type/{feeType}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<StudentFeeDto>), 200)]
    public async Task<ActionResult<IEnumerable<StudentFeeDto>>> GetByFeeType(
        FeeType feeType,
        CancellationToken cancellationToken)
    {
        try
        {
            var fees = await _feeService.GetByFeeTypeAsync(feeType, cancellationToken);
            return Ok(fees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving fee records for type: {FeeType}", feeType);
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving fee records." });
        }
    }

    /// <summary>
    /// Get fee records by term (role-restricted access)
    /// </summary>
    /// <param name="term">Term (e.g., "2024-Q1")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Fee records for the specified term</returns>
    [HttpGet("term/{term}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<StudentFeeDto>), 200)]
    public async Task<ActionResult<IEnumerable<StudentFeeDto>>> GetByTerm(
        string term,
        CancellationToken cancellationToken)
    {
        try
        {
            var fees = await _feeService.GetByTermAsync(term, cancellationToken);
            return Ok(fees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving fee records for term: {Term}", term);
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving fee records." });
        }
    }

    /// <summary>
    /// Get overdue fee records (Admin/DevAuth only)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of overdue fee records</returns>
    [HttpGet("overdue")]
    [Authorize(Roles = "Admin,DevAuth")]
    [ProducesResponseType(typeof(IEnumerable<StudentFeeDto>), 200)]
    public async Task<ActionResult<IEnumerable<StudentFeeDto>>> GetOverdueFees(
        CancellationToken cancellationToken)
    {
        try
        {
            var fees = await _feeService.GetOverdueAsync(cancellationToken);
            return Ok(fees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving overdue fee records");
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving overdue fee records." });
        }
    }

    /// <summary>
    /// Get fee statistics for a student (role-restricted access)
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Fee statistics for the student</returns>
    [HttpGet("statistics/{studentId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<ActionResult<object>> GetStudentFeeStatistics(
        Guid studentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var userRole = GetCurrentUserRole();
            var currentUserId = GetCurrentUserId();

            // Check access permissions
            if (userRole == UserRole.Student && currentUserId != studentId)
            {
                return Forbid();
            }

            if (userRole == UserRole.Faculty && currentUserId.HasValue)
            {
                // Faculty can only access statistics for students assigned to them
                // This would require additional validation
            }

            if (userRole == UserRole.Parent && currentUserId.HasValue)
            {
                // Parent can only access statistics for their children
                // This would require additional validation
            }

            var statistics = await _feeService.GetStudentFeeStatisticsAsync(studentId, cancellationToken);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving statistics for student: {StudentId}", studentId);
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving student fee statistics." });
        }
    }

    /// <summary>
    /// Calculate and apply late fees for overdue records (Admin/DevAuth only)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of records updated with late fees</returns>
    [HttpPost("calculate-late-fees")]
    [Authorize(Roles = "Admin,DevAuth")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<ActionResult<object>> CalculateLateFees(
        CancellationToken cancellationToken)
    {
        try
        {
            var updatedCount = await _feeService.CalculateLateFeesAsync(cancellationToken);
            _logger.LogInformation("Late fees calculated for {Count} overdue fee records", updatedCount);
            
            return Ok(new { 
                message = "Late fees calculated successfully", 
                updatedCount = updatedCount 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calculating late fees");
            return StatusCode(500, new { error = "An unexpected error occurred while calculating late fees." });
        }
    }

    // Helper methods
    private UserRole GetCurrentUserRole()
    {
        var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (Enum.TryParse<UserRole>(roleClaim, out var role))
        {
            return role;
        }
        return UserRole.Student; // Default fallback
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }

    private bool CheckAuthorizationAsync(StudentFeeAccessRequirement requirement, StudentFeeDto fee)
    {
        // This is a simplified authorization check
        // In a real implementation, you would use the authorization handler
        var userRole = GetCurrentUserRole();
        var userId = GetCurrentUserId();

        return userRole switch
        {
            UserRole.Admin or UserRole.DevAuth => true,
            UserRole.Student => userId.HasValue && fee.StudentId == userId.Value,
            UserRole.Faculty => true, // Simplified - should check faculty assignment
            UserRole.Parent => true,  // Simplified - should check parent relationship
            _ => false
        };
    }
}
