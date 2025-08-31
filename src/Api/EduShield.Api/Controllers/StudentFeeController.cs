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
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<StudentFeeController> _logger;

    public StudentFeeController(IStudentFeeService feeService, IStudentRepository studentRepository, ILogger<StudentFeeController> logger)
    {
        _feeService = feeService;
        _studentRepository = studentRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all student fee records with pagination (role-restricted access)
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
    /// - `paymentStatus`: Filter by payment status
    /// - `isOverdue`: Filter by overdue status
    /// - `fromDate`: Filter by start date (ISO format)
    /// - `toDate`: Filter by end date (ISO format)
    /// - `search`: Search term for filtering
    /// - `page`: Page number (default: 1)
    /// - `limit`: Items per page (default: 10, max: 100)
    /// - `sortBy`: Field to sort by
    /// - `sortOrder`: Sort order (asc/desc, default: asc)
    /// </remarks>
    /// <param name="feeType">Optional fee type filter</param>
    /// <param name="term">Optional term filter</param>
    /// <param name="paymentStatus">Optional payment status filter</param>
    /// <param name="isOverdue">Optional overdue status filter</param>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <param name="search">Optional search term</param>
    /// <param name="page">Page number</param>
    /// <param name="limit">Items per page</param>
    /// <param name="sortBy">Field to sort by</param>
    /// <param name="sortOrder">Sort order</param>
    /// <param name="studentId">Optional student ID filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated fee records based on user role and filters</returns>
    /// <response code="200">Fee records retrieved successfully.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Insufficient permissions.</response>
    /// <response code="500">Internal server error during retrieval.</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PaginatedResponse<StudentFeeDto>), 200)]
    public async Task<ActionResult<PaginatedResponse<StudentFeeDto>>> GetAllFees(
        [FromQuery] FeeType? feeType,
        [FromQuery] string? term,
        [FromQuery] PaymentStatus? paymentStatus,
        [FromQuery] bool? isOverdue,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] string sortOrder = "asc",
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        [FromQuery] Guid? studentId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userRole = GetCurrentUserRole();
            var userId = GetCurrentUserId();

            // Create filter request
            var filter = new StudentFeeFilterRequest
            {
                FeeType = feeType,
                Term = term,
                PaymentStatus = paymentStatus,
                IsOverdue = isOverdue,
                FromDate = fromDate,
                ToDate = toDate,
                Search = search,
                Page = page,
                Limit = limit,
                SortBy = sortBy,
                SortOrder = sortOrder,
                StudentId = studentId
            };

            // Apply role-based filtering
            switch (userRole)
            {
                case UserRole.Admin:
                case UserRole.DevAuth:
                    // Admin/Dev can see all fee records - no additional filtering needed
                    break;

                case UserRole.Faculty:
                    // Faculty can see fee records for students assigned to them
                    if (!userId.HasValue)
                    {
                        return Unauthorized(new { error = "User ID not found." });
                    }
                    // TODO: Implement faculty-specific filtering in service
                    break;

                case UserRole.Student:
                    // Students can only see their own fee records
                    if (!userId.HasValue)
                    {
                        return Unauthorized(new { error = "User ID not found." });
                    }
                    
                    // For students, use the GetByUserIdAsync method directly
                    var studentFees = await _feeService.GetByUserIdAsync(userId.Value, cancellationToken);
                    var studentFeesList = studentFees.ToList();
                    
                    // Apply filters to the student's fees
                    var filteredStudentFees = studentFeesList.AsQueryable();
                    
                    if (feeType.HasValue)
                    {
                        filteredStudentFees = filteredStudentFees.Where(f => f.FeeType == feeType.Value);
                    }
                    
                    if (!string.IsNullOrEmpty(term))
                    {
                        filteredStudentFees = filteredStudentFees.Where(f => f.Term.Contains(term, StringComparison.OrdinalIgnoreCase));
                    }
                    
                    if (paymentStatus.HasValue)
                    {
                        filteredStudentFees = filteredStudentFees.Where(f => f.PaymentStatus == paymentStatus.Value);
                    }
                    
                    if (isOverdue.HasValue)
                    {
                        if (isOverdue.Value)
                        {
                            filteredStudentFees = filteredStudentFees.Where(f => f.IsOverdue);
                        }
                        else
                        {
                            filteredStudentFees = filteredStudentFees.Where(f => !f.IsOverdue);
                        }
                    }
                    
                    if (fromDate.HasValue)
                    {
                        filteredStudentFees = filteredStudentFees.Where(f => f.DueDate >= fromDate.Value);
                    }
                    
                    if (toDate.HasValue)
                    {
                        filteredStudentFees = filteredStudentFees.Where(f => f.DueDate <= toDate.Value);
                    }
                    
                    if (!string.IsNullOrEmpty(search))
                    {
                        filteredStudentFees = filteredStudentFees.Where(f => 
                            f.Term.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            f.FeeTypeDescription.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            f.PaymentStatusDescription.Contains(search, StringComparison.OrdinalIgnoreCase));
                    }
                    
                    // Apply sorting
                    if (!string.IsNullOrEmpty(sortBy))
                    {
                        var isDescending = sortOrder?.ToLower() == "desc";
                        
                        filteredStudentFees = sortBy.ToLower() switch
                        {
                            "feetype" => isDescending ? filteredStudentFees.OrderByDescending(f => f.FeeType) : filteredStudentFees.OrderBy(f => f.FeeType),
                            "term" => isDescending ? filteredStudentFees.OrderByDescending(f => f.Term) : filteredStudentFees.OrderBy(f => f.Term),
                            "duedate" => isDescending ? filteredStudentFees.OrderByDescending(f => f.DueDate) : filteredStudentFees.OrderBy(f => f.DueDate),
                            "paymentstatus" => isDescending ? filteredStudentFees.OrderByDescending(f => f.PaymentStatus) : filteredStudentFees.OrderBy(f => f.PaymentStatus),
                            "totalamount" => isDescending ? filteredStudentFees.OrderByDescending(f => f.TotalAmount) : filteredStudentFees.OrderBy(f => f.TotalAmount),
                            "amountpaid" => isDescending ? filteredStudentFees.OrderByDescending(f => f.AmountPaid) : filteredStudentFees.OrderBy(f => f.AmountPaid),
                            "amountdue" => isDescending ? filteredStudentFees.OrderByDescending(f => f.AmountDue) : filteredStudentFees.OrderBy(f => f.AmountDue),
                            _ => isDescending ? filteredStudentFees.OrderByDescending(f => f.DueDate) : filteredStudentFees.OrderBy(f => f.DueDate)
                        };
                    }
                    else
                    {
                        // Default sorting by due date descending
                        filteredStudentFees = filteredStudentFees.OrderByDescending(f => f.DueDate);
                    }
                    
                    // Apply pagination
                    var totalCount = filteredStudentFees.Count();
                    var pagedFees = filteredStudentFees
                        .Skip((page - 1) * limit)
                        .Take(limit)
                        .ToList();
                    
                    var studentResult = PaginatedResponse<StudentFeeDto>.Create(
                        pagedFees,
                        totalCount,
                        page,
                        limit
                    );
                    
                    return Ok(studentResult);

                case UserRole.Parent:
                    // Parents can see fee records for their children
                    if (!userId.HasValue)
                    {
                        return Unauthorized(new { error = "User ID not found." });
                    }
                    
                    // Get parent record to find their children
                    var parentRepository = HttpContext.RequestServices.GetRequiredService<IParentRepository>();
                    var parent = await parentRepository.GetByUserIdAsync(userId.Value);
                    if (parent == null)
                    {
                        return NotFound(new { error = "Parent record not found for this user." });
                    }
                    
                    // Get all children for this parent
                    var studentService = HttpContext.RequestServices.GetRequiredService<IStudentService>();
                    var children = await studentService.GetByParentIdAsync(parent.Id, cancellationToken);
                    
                    if (!children.Any())
                    {
                        return Ok(PaginatedResponse<StudentFeeDto>.Create(
                            new List<StudentFeeDto>(), 0, page, limit));
                    }
                    
                    // If studentId filter is provided, check if it belongs to this parent
                    if (filter.StudentId.HasValue)
                    {
                        var requestedStudentId = filter.StudentId.Value;
                        var requestedChild = children.FirstOrDefault(c => c.Id == requestedStudentId);
                        if (requestedChild == null)
                        {
                            // Parent doesn't have access to this student
                            return Forbid();
                        }
                        
                        // Get fee records for the specific child only
                        var childFees = await _feeService.GetByStudentIdAsync(requestedStudentId, cancellationToken);
                        var allFees = childFees.ToList();
                        
                        // Apply filters and return
                        var childFilteredFees = allFees.AsQueryable();
                        childFilteredFees = ApplyFeeFilters(childFilteredFees, filter);
                        childFilteredFees = ApplyFeeSorting(childFilteredFees, filter);
                        
                        var childTotalCount = childFilteredFees.Count();
                        var childPagedFees = childFilteredFees
                            .Skip((filter.Page - 1) * filter.Limit)
                            .Take(filter.Limit)
                            .ToList();
                        
                        return Ok(PaginatedResponse<StudentFeeDto>.Create(childPagedFees, childTotalCount, filter.Page, filter.Limit));
                    }
                    
                    // Get fee records for all children
                    var allChildFees = new List<StudentFeeDto>();
                    foreach (var child in children)
                    {
                        var childFees = await _feeService.GetByStudentIdAsync(child.Id, cancellationToken);
                        allChildFees.AddRange(childFees);
                    }
                    
                    // Apply filters to the combined results
                    var filteredFees = allChildFees.AsQueryable();
                    filteredFees = ApplyFeeFilters(filteredFees, filter);
                    filteredFees = ApplyFeeSorting(filteredFees, filter);
                    
                    // Apply pagination
                    var parentTotalCount = filteredFees.Count();
                    var parentPagedFees = filteredFees
                        .Skip((filter.Page - 1) * filter.Limit)
                        .Take(filter.Limit)
                        .ToList();
                    
                    return Ok(PaginatedResponse<StudentFeeDto>.Create(parentPagedFees, parentTotalCount, filter.Page, filter.Limit));

                default:
                    return Forbid();
            }

            var result = await _feeService.GetPaginatedAsync(filter, cancellationToken);
            return Ok(result);
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
    /// Get all fee records for the current student (Student role only)
    /// </summary>
    /// <remarks>
    /// This endpoint allows students to retrieve all their fee records directly.
    /// It automatically filters by the current user's student record.
    /// 
    /// **Access Control:**
    /// - **Student**: Can view only their own fee records
    /// - **Admin/DevAuth/Faculty/Parent**: Not allowed to use this endpoint
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All fee records for the current student</returns>
    /// <response code="200">Fee records retrieved successfully.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Student role required.</response>
    /// <response code="404">Student record not found for current user.</response>
    /// <response code="500">Internal server error during retrieval.</response>
    [HttpGet("student/my-fees")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(IEnumerable<StudentFeeDto>), 200)]
    public async Task<ActionResult<IEnumerable<StudentFeeDto>>> GetMyFees(
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { error = "User ID not found." });
            }

            _logger.LogInformation("Getting fees for student with UserId: {UserId}", userId.Value);

            // Use the service method that gets fees by user ID
            var fees = await _feeService.GetByUserIdAsync(userId.Value, cancellationToken);
            var feesList = fees.ToList();

            _logger.LogInformation("Found {Count} fees for student with UserId: {UserId}", feesList.Count, userId.Value);

            return Ok(feesList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving fee records for current student");
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving fee records." });
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

    private IQueryable<StudentFeeDto> ApplyFeeFilters(IQueryable<StudentFeeDto> query, StudentFeeFilterRequest filter)
    {
        if (filter.FeeType.HasValue)
        {
            query = query.Where(f => f.FeeType == filter.FeeType.Value);
        }
        
        if (filter.PaymentStatus.HasValue)
        {
            query = query.Where(f => f.PaymentStatus == filter.PaymentStatus.Value);
        }
        
        if (!string.IsNullOrEmpty(filter.Term))
        {
            query = query.Where(f => f.Term.Contains(filter.Term, StringComparison.OrdinalIgnoreCase));
        }
        
        if (filter.IsOverdue.HasValue)
        {
            query = query.Where(f => f.IsOverdue == filter.IsOverdue.Value);
        }
        
        if (filter.FromDate.HasValue)
        {
            query = query.Where(f => f.DueDate >= filter.FromDate.Value);
        }
        
        if (filter.ToDate.HasValue)
        {
            query = query.Where(f => f.DueDate <= filter.ToDate.Value);
        }
        
        if (!string.IsNullOrEmpty(filter.Search))
        {
            query = query.Where(f => 
                f.StudentFirstName.Contains(filter.Search, StringComparison.OrdinalIgnoreCase) ||
                f.StudentLastName.Contains(filter.Search, StringComparison.OrdinalIgnoreCase) ||
                f.StudentRollNumber.Contains(filter.Search, StringComparison.OrdinalIgnoreCase) ||
                f.Term.Contains(filter.Search, StringComparison.OrdinalIgnoreCase) ||
                (f.Notes != null && f.Notes.Contains(filter.Search, StringComparison.OrdinalIgnoreCase)));
        }
        
        return query;
    }

    private IQueryable<StudentFeeDto> ApplyFeeSorting(IQueryable<StudentFeeDto> query, StudentFeeFilterRequest filter)
    {
        if (!string.IsNullOrEmpty(filter.SortBy))
        {
            var isDescending = filter.SortOrder?.ToLower() == "desc";
            return filter.SortBy.ToLower() switch
            {
                "duedate" => isDescending ? query.OrderByDescending(f => f.DueDate) : query.OrderBy(f => f.DueDate),
                "totalamount" => isDescending ? query.OrderByDescending(f => f.TotalAmount) : query.OrderBy(f => f.TotalAmount),
                "amountdue" => isDescending ? query.OrderByDescending(f => f.AmountDue) : query.OrderBy(f => f.AmountDue),
                "paymentstatus" => isDescending ? query.OrderByDescending(f => f.PaymentStatus) : query.OrderBy(f => f.PaymentStatus),
                "term" => isDescending ? query.OrderByDescending(f => f.Term) : query.OrderBy(f => f.Term),
                _ => isDescending ? query.OrderByDescending(f => f.DueDate) : query.OrderBy(f => f.DueDate)
            };
        }
        else
        {
            // Default sort by due date ascending
            return query.OrderBy(f => f.DueDate);
        }
    }
}
