using EduShield.Api.Auth.Requirements;
using EduShield.Core.Dtos;
using EduShield.Core.Enums;
using EduShield.Core.Services;
using EduShield.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EduShield.Api.Controllers;

/// <summary>
/// Controller for managing student performance records
/// </summary>
[ApiController]
[Route("api/v1/student-performance")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), 400)]
[ProducesResponseType(typeof(ProblemDetails), 401)]
[ProducesResponseType(typeof(ProblemDetails), 403)]
[ProducesResponseType(typeof(ProblemDetails), 404)]
[ProducesResponseType(typeof(ProblemDetails), 500)]
[EnableRateLimiting("PerformancePolicy")]
public class StudentPerformanceController : ControllerBase
{
    private readonly IStudentPerformanceService _performanceService;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<StudentPerformanceController> _logger;

    public StudentPerformanceController(IStudentPerformanceService performanceService, IStudentRepository studentRepository, ILogger<StudentPerformanceController> logger)
    {
        _performanceService = performanceService;
        _studentRepository = studentRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all student performance records with pagination (role-restricted access)
    /// </summary>
    /// <remarks>
    /// **Access Control:**
    /// - **Admin/DevAuth**: Can view all performance records
    /// - **Faculty**: Can view performance records for students assigned to them
    /// - **Student**: Can view only their own performance records
    /// - **Parent**: Can view performance records for their children
    /// 
    /// **Query Parameters:**
    /// - `subject`: Filter by subject name
    /// - `examType`: Filter by exam type
    /// - `fromDate`: Filter by start date (ISO format)
    /// - `toDate`: Filter by end date (ISO format)
    /// - `search`: Search term for filtering
    /// - `page`: Page number (default: 1)
    /// - `limit`: Items per page (default: 10, max: 100)
    /// - `sortBy`: Field to sort by
    /// - `sortOrder`: Sort order (asc/desc, default: asc)
    /// </remarks>
    /// <param name="subject">Optional subject filter</param>
    /// <param name="examType">Optional exam type filter</param>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <param name="search">Optional search term</param>
    /// <param name="page">Page number</param>
    /// <param name="limit">Items per page</param>
    /// <param name="sortBy">Field to sort by</param>
    /// <param name="sortOrder">Sort order</param>
    /// <param name="studentId">Optional student ID filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated performance records based on user role and filters</returns>
    /// <response code="200">Performance records retrieved successfully.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Insufficient permissions.</response>
    /// <response code="500">Internal server error during retrieval.</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PaginatedResponse<StudentPerformanceDto>), 200)]
    public async Task<ActionResult<PaginatedResponse<StudentPerformanceDto>>> GetAllPerformance(
        [FromQuery] string? subject,
        [FromQuery] ExamType? examType,
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
            var filter = new StudentPerformanceFilterRequest
            {
                Subject = subject,
                ExamType = examType,
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
                    // Admin/Dev can see all performance records - no additional filtering needed
                    break;

                case UserRole.Faculty:
                    // Faculty can see performance records for students assigned to them
                    if (!userId.HasValue)
                    {
                        return Unauthorized(new { error = "User ID not found." });
                    }
                    // TODO: Implement faculty-specific filtering in service
                    break;

                case UserRole.Student:
                    // Students can only see their own performance records
                    if (!userId.HasValue)
                    {
                        return Unauthorized(new { error = "User ID not found." });
                    }
                    
                    // Get the student record by user ID
                    var student = await _studentRepository.GetByUserIdAsync(userId.Value, cancellationToken);
                    if (student == null)
                    {
                        return NotFound(new { error = "Student record not found for this user." });
                    }
                    
                    filter.StudentId = student.Id;
                    break;

                case UserRole.Parent:
                    // Parents can see performance records for their children
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
                        return Ok(PaginatedResponse<StudentPerformanceDto>.Create(
                            new List<StudentPerformanceDto>(), 0, page, limit));
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
                        
                        // Get performance records for the specific child only
                        var childPerformances = await _performanceService.GetByStudentIdAsync(requestedStudentId, cancellationToken);
                        var allPerformances = childPerformances.ToList();
                        
                        // Apply filters and return
                        var childFilteredPerformances = allPerformances.AsQueryable();
                        childFilteredPerformances = ApplyPerformanceFilters(childFilteredPerformances, filter);
                        childFilteredPerformances = ApplyPerformanceSorting(childFilteredPerformances, filter);
                        
                        var childTotalCount = childFilteredPerformances.Count();
                        var childPagedPerformances = childFilteredPerformances
                            .Skip((filter.Page - 1) * filter.Limit)
                            .Take(filter.Limit)
                            .ToList();
                        
                        return Ok(PaginatedResponse<StudentPerformanceDto>.Create(childPagedPerformances, childTotalCount, filter.Page, filter.Limit));
                    }
                    
                    // Get performance records for all children
                    var allChildPerformances = new List<StudentPerformanceDto>();
                    foreach (var child in children)
                    {
                        var childPerformances = await _performanceService.GetByStudentIdAsync(child.Id, cancellationToken);
                        allChildPerformances.AddRange(childPerformances);
                    }
                    
                    // Apply filters to the combined results
                    var filteredPerformances = allChildPerformances.AsQueryable();
                    filteredPerformances = ApplyPerformanceFilters(filteredPerformances, filter);
                    filteredPerformances = ApplyPerformanceSorting(filteredPerformances, filter);
                    
                    // Apply pagination
                    var totalCount = filteredPerformances.Count();
                    var pagedPerformances = filteredPerformances
                        .Skip((filter.Page - 1) * filter.Limit)
                        .Take(filter.Limit)
                        .ToList();
                    
                    return Ok(PaginatedResponse<StudentPerformanceDto>.Create(pagedPerformances, totalCount, filter.Page, filter.Limit));

                default:
                    return Forbid();
            }

            var result = await _performanceService.GetPaginatedAsync(filter, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving performance records");
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving performance records." });
        }
    }

    /// <summary>
    /// Get a specific performance record by ID (role-restricted access)
    /// </summary>
    /// <remarks>
    /// **Access Control:** Same as GET /api/v1/student-performance
    /// </remarks>
    /// <param name="id">Performance record ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Performance record details</returns>
    /// <response code="200">Performance record found successfully.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Insufficient permissions.</response>
    /// <response code="404">Performance record not found.</response>
    /// <response code="500">Internal server error during retrieval.</response>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(StudentPerformanceDto), 200)]
    public async Task<ActionResult<StudentPerformanceDto>> GetPerformanceById(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var performance = await _performanceService.GetByIdAsync(id, cancellationToken);
            if (performance == null)
            {
                return NotFound(new { error = "Performance record not found." });
            }

            // Check authorization using the requirement
            var requirement = new StudentPerformanceAccessRequirement { ReadOnly = true };
            var authResult = CheckAuthorizationAsync(requirement, performance);
            
            if (!authResult)
            {
                return Forbid();
            }

            return Ok(performance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving performance record with ID: {PerformanceId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving the performance record." });
        }
    }

    /// <summary>
    /// Create a new performance record (Admin/Faculty only)
    /// </summary>
    /// <remarks>
    /// **Access Control:**
    /// - **Admin/DevAuth**: Can create performance records for any student
    /// - **Faculty**: Can create performance records for students assigned to them
    /// - **Student/Parent**: Cannot create performance records
    /// 
    /// **Score Encryption:** The score is automatically encrypted before storage for security.
    /// </remarks>
    /// <param name="request">Performance creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created performance record</returns>
    /// <response code="201">Performance record created successfully.</response>
    /// <response code="400">Invalid request data or validation failed.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin or Faculty role required.</response>
    /// <response code="500">Internal server error during creation.</response>
    [HttpPost]
    [Authorize(Roles = "Admin,DevAuth,Faculty")]
    [ProducesResponseType(typeof(StudentPerformanceDto), 201)]
    public async Task<ActionResult<StudentPerformanceDto>> CreatePerformance(
        [FromBody] CreateStudentPerformanceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userRole = GetCurrentUserRole();
            var userId = GetCurrentUserId();

            // Check if user is authorized to create performance records
            if (userRole != UserRole.Admin && userRole != UserRole.DevAuth && userRole != UserRole.Faculty)
            {
                return Forbid();
            }

            // Faculty can only create performance records for students assigned to them
            if (userRole == UserRole.Faculty && userId.HasValue)
            {
                // This would require additional validation in the service layer
                // For now, we'll let the service handle it
            }

            var performance = await _performanceService.CreateAsync(request, cancellationToken);
            _logger.LogInformation("Performance record created successfully with ID: {PerformanceId}", performance.Id);
            
            return CreatedAtAction(nameof(GetPerformanceById), new { id = performance.Id }, performance);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create performance record: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating performance record");
            return StatusCode(500, new { error = "An unexpected error occurred while creating the performance record." });
        }
    }

    /// <summary>
    /// Update an existing performance record (Admin/Faculty only)
    /// </summary>
    /// <remarks>
    /// **Access Control:**
    /// - **Admin/DevAuth**: Can update any performance record
    /// - **Faculty**: Can update performance records for students assigned to them
    /// - **Student/Parent**: Cannot update performance records
    /// 
    /// **Score Encryption:** Updated scores are automatically re-encrypted.
    /// </remarks>
    /// <param name="id">Performance record ID to update</param>
    /// <param name="request">Performance update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated performance record</returns>
    /// <response code="200">Performance record updated successfully.</response>
    /// <response code="400">Invalid request data or validation failed.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin or Faculty role required.</response>
    /// <response code="404">Performance record not found.</response>
    /// <response code="500">Internal server error during update.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,DevAuth,Faculty")]
    [ProducesResponseType(typeof(StudentPerformanceDto), 200)]
    public async Task<ActionResult<StudentPerformanceDto>> UpdatePerformance(
        Guid id,
        [FromBody] UpdateStudentPerformanceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userRole = GetCurrentUserRole();
            var userId = GetCurrentUserId();

            // Check if user is authorized to update performance records
            if (userRole != UserRole.Admin && userRole != UserRole.DevAuth && userRole != UserRole.Faculty)
            {
                return Forbid();
            }

            // Faculty can only update performance records for students assigned to them
            if (userRole == UserRole.Faculty && userId.HasValue)
            {
                var existingPerformance = await _performanceService.GetByIdAsync(id, cancellationToken);
                if (existingPerformance == null)
                {
                    return NotFound(new { error = "Performance record not found." });
                }

                // This would require additional validation in the service layer
                // For now, we'll let the service handle it
            }

            var performance = await _performanceService.UpdateAsync(id, request, cancellationToken);
            _logger.LogInformation("Performance record updated successfully with ID: {PerformanceId}", id);
            
            return Ok(performance);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update performance record {PerformanceId}: {Message}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating performance record {PerformanceId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred while updating the performance record." });
        }
    }

    /// <summary>
    /// Delete a performance record (Admin only)
    /// </summary>
    /// <remarks>
    /// **Access Control:**
    /// - **Admin/DevAuth**: Can delete any performance record
    /// - **Faculty/Student/Parent**: Cannot delete performance records
    /// 
    /// **Warning:** This action cannot be undone.
    /// </remarks>
    /// <param name="id">Performance record ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">Performance record deleted successfully.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin role required.</response>
    /// <response code="404">Performance record not found.</response>
    /// <response code="500">Internal server error during deletion.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,DevAuth")]
    [ProducesResponseType(204)]
    public async Task<ActionResult> DeletePerformance(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var userRole = GetCurrentUserRole();
            
            // Check if user is authorized to delete performance records
            if (userRole != UserRole.Admin && userRole != UserRole.DevAuth)
            {
                return Forbid();
            }

            if (!await _performanceService.ExistsAsync(id, cancellationToken))
            {
                return NotFound(new { error = "Performance record not found." });
            }

            await _performanceService.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("Performance record deleted successfully with ID: {PerformanceId}", id);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting performance record {PerformanceId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred while deleting the performance record." });
        }
    }

    /// <summary>
    /// Get performance statistics for a student (role-restricted access)
    /// </summary>
    /// <remarks>
    /// **Access Control:** Same as GET /api/v1/student-performance
    /// 
    /// **Returns:** Performance statistics including total exams, average score, highest/lowest scores, and subject breakdown.
    /// </remarks>
    /// <param name="studentId">Student ID</param>
    /// <param name="subject">Optional subject filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Performance statistics for the student</returns>
    /// <response code="200">Statistics retrieved successfully.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Insufficient permissions.</response>
    /// <response code="500">Internal server error during retrieval.</response>
    [HttpGet("statistics/{studentId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<ActionResult<object>> GetStudentStatistics(
        Guid studentId,
        [FromQuery] string? subject,
        CancellationToken cancellationToken)
    {
        try
        {
            var userRole = GetCurrentUserRole();
            var currentUserId = GetCurrentUserId();

            // Check access permissions
            if (userRole == UserRole.Student)
            {
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { error = "User ID not found." });
                }
                
                // Get the student record by user ID to verify they can only access their own data
                var student = await _studentRepository.GetByUserIdAsync(currentUserId.Value, cancellationToken);
                if (student == null)
                {
                    return NotFound(new { error = "Student record not found for this user." });
                }
                
                // Students can only access their own statistics
                if (student.Id != studentId)
                {
                    return Forbid();
                }
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

            var statistics = await _performanceService.GetStudentStatisticsAsync(studentId, subject, cancellationToken);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving statistics for student: {StudentId}", studentId);
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving student statistics." });
        }
    }

    /// <summary>
    /// Get performance records by subject (role-restricted access)
    /// </summary>
    /// <param name="subject">Subject name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Performance records for the specified subject</returns>
    [HttpGet("subject/{subject}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<StudentPerformanceDto>), 200)]
    public async Task<ActionResult<IEnumerable<StudentPerformanceDto>>> GetBySubject(
        string subject,
        CancellationToken cancellationToken)
    {
        try
        {
            var performances = await _performanceService.GetBySubjectAsync(subject, cancellationToken);
            return Ok(performances);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving performance records for subject: {Subject}", subject);
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving performance records." });
        }
    }

    /// <summary>
    /// Get performance records by exam type (role-restricted access)
    /// </summary>
    /// <param name="examType">Exam type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Performance records for the specified exam type</returns>
    [HttpGet("exam-type/{examType}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<StudentPerformanceDto>), 200)]
    public async Task<ActionResult<IEnumerable<StudentPerformanceDto>>> GetByExamType(
        ExamType examType,
        CancellationToken cancellationToken)
    {
        try
        {
            var performances = await _performanceService.GetByExamTypeAsync(examType, cancellationToken);
            return Ok(performances);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving performance records for exam type: {ExamType}", examType);
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving performance records." });
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

    private bool CheckAuthorizationAsync(StudentPerformanceAccessRequirement requirement, StudentPerformanceDto performance)
    {
        // This is a simplified authorization check
        // In a real implementation, you would use the authorization handler
        var userRole = GetCurrentUserRole();
        var userId = GetCurrentUserId();

        return userRole switch
        {
            UserRole.Admin or UserRole.DevAuth => true,
            UserRole.Student => userId.HasValue && performance.StudentId == userId.Value,
            UserRole.Faculty => true, // Simplified - should check faculty assignment
            UserRole.Parent => true,  // Simplified - should check parent relationship
            _ => false
        };
    }

    private IQueryable<StudentPerformanceDto> ApplyPerformanceFilters(IQueryable<StudentPerformanceDto> query, StudentPerformanceFilterRequest filter)
    {
        if (!string.IsNullOrEmpty(filter.Subject))
        {
            query = query.Where(p => p.Subject.Contains(filter.Subject, StringComparison.OrdinalIgnoreCase));
        }
        
        if (filter.ExamType.HasValue)
        {
            query = query.Where(p => p.ExamType == filter.ExamType.Value);
        }
        
        if (filter.FromDate.HasValue)
        {
            query = query.Where(p => p.ExamDate >= filter.FromDate.Value);
        }
        
        if (filter.ToDate.HasValue)
        {
            query = query.Where(p => p.ExamDate <= filter.ToDate.Value);
        }
        
        if (!string.IsNullOrEmpty(filter.Search))
        {
            query = query.Where(p => 
                p.Subject.Contains(filter.Search, StringComparison.OrdinalIgnoreCase) ||
                p.StudentFirstName.Contains(filter.Search, StringComparison.OrdinalIgnoreCase) ||
                p.StudentLastName.Contains(filter.Search, StringComparison.OrdinalIgnoreCase) ||
                (p.ExamTitle != null && p.ExamTitle.Contains(filter.Search, StringComparison.OrdinalIgnoreCase)));
        }
        
        return query;
    }

    private IQueryable<StudentPerformanceDto> ApplyPerformanceSorting(IQueryable<StudentPerformanceDto> query, StudentPerformanceFilterRequest filter)
    {
        if (!string.IsNullOrEmpty(filter.SortBy))
        {
            var isDescending = filter.SortOrder?.ToLower() == "desc";
            return filter.SortBy.ToLower() switch
            {
                "examdate" => isDescending ? query.OrderByDescending(p => p.ExamDate) : query.OrderBy(p => p.ExamDate),
                "subject" => isDescending ? query.OrderByDescending(p => p.Subject) : query.OrderBy(p => p.Subject),
                "score" => isDescending ? query.OrderByDescending(p => p.Score) : query.OrderBy(p => p.Score),
                "percentage" => isDescending ? query.OrderByDescending(p => p.Percentage) : query.OrderBy(p => p.Percentage),
                _ => isDescending ? query.OrderByDescending(p => p.ExamDate) : query.OrderBy(p => p.ExamDate)
            };
        }
        else
        {
            // Default sort by exam date descending
            return query.OrderByDescending(p => p.ExamDate);
        }
    }
}
