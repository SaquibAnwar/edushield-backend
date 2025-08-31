using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduShield.Core.Data;
using EduShield.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduShield.Api.Controllers;

/// <summary>
/// Controller for retrieving system metrics and dashboard data
/// </summary>
[ApiController]
[Route("api/v1/metrics")]
[Authorize]
public class MetricsController : ControllerBase
{
    private readonly EduShieldDbContext _context;
    private readonly ILogger<MetricsController> _logger;

    public MetricsController(EduShieldDbContext context, ILogger<MetricsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get current user's parent metrics for dashboard
    /// </summary>
    /// <remarks>
    /// Retrieves comprehensive metrics for the currently authenticated parent including:
    /// - Total number of children
    /// - Children with overdue fees
    /// - Total overdue amount
    /// - Recent academic performances (last 30 days)
    /// 
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "totalChildren": 2,
    ///   "childrenWithOverdueFees": 1,
    ///   "totalOverdueAmount": 150.00,
    ///   "recentPerformances": [
    ///     {
    ///       "id": "guid",
    ///       "studentId": "guid",
    ///       "studentFirstName": "John",
    ///       "studentLastName": "Doe",
    ///       "subject": "Mathematics",
    ///       "examType": "MidTerm",
    ///       "examDate": "2024-01-15T00:00:00Z",
    ///       "score": 85,
    ///       "maxScore": 100,
    ///       "percentage": 85.0,
    ///       "grade": "A",
    ///       "formattedScore": "85/100",
    ///       "examTitle": "Mid-term Mathematics Exam",
    ///       "comments": "Good performance",
    ///       "createdAt": "2024-01-15T10:30:00Z",
    ///       "updatedAt": "2024-01-15T10:30:00Z"
    ///     }
    ///   ]
    /// }
    /// ```
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Parent metrics data</returns>
    /// <response code="200">Metrics retrieved successfully.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Parent role required.</response>
    /// <response code="404">Parent profile not found for current user.</response>
    /// <response code="500">Internal server error during metrics retrieval.</response>
    [HttpGet("parent")]
    [Authorize(Policy = "ParentOnly")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 403)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<ActionResult> GetCurrentUserParentMetrics(CancellationToken cancellationToken)
    {
        try
        {
            // Get current user ID from claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "User ID not found in token." });
            }

            // Find parent by user ID
            var parent = await _context.Parents
                .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

            if (parent == null)
            {
                return NotFound(new { error = "Parent profile not found for current user." });
            }

            return await GetParentMetricsInternal(parent.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving current user parent metrics");
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving parent metrics." });
        }
    }

    /// <summary>
    /// Get parent metrics for dashboard by parent ID
    /// </summary>
    /// <remarks>
    /// Retrieves comprehensive metrics for a specific parent (Admin access or parent accessing their own data).
    /// Returns the same data structure as the current user endpoint.
    /// </remarks>
    /// <param name="parentId">Parent ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Parent metrics data</returns>
    /// <response code="200">Metrics retrieved successfully.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin role or parent accessing own data required.</response>
    /// <response code="404">Parent not found.</response>
    /// <response code="500">Internal server error during metrics retrieval.</response>
    [HttpGet("parent/{parentId:guid}")]
    [Authorize(Policy = "AdminOrParent")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 403)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<ActionResult> GetParentMetrics(Guid parentId, CancellationToken cancellationToken)
    {
        return await GetParentMetricsInternal(parentId, cancellationToken);
    }

    private async Task<ActionResult> GetParentMetricsInternal(Guid parentId, CancellationToken cancellationToken)
    {
        try
        {
            // Get all children for this parent
            var children = await _context.Students
                .Where(s => _context.ParentStudents
                    .Any(ps => ps.ParentId == parentId && ps.StudentId == s.Id && ps.IsActive))
                .ToListAsync(cancellationToken);

            var totalChildren = children.Count;

            // Get overdue fees for parent's children
            var childrenIds = children.Select(c => c.Id).ToList();
            var overdueFees = await _context.StudentFees
                .Where(f => childrenIds.Contains(f.StudentId) && 
                           f.DueDate < DateTime.UtcNow && 
                           f.PaymentStatus != PaymentStatus.Paid)
                .ToListAsync(cancellationToken);

            var childrenWithOverdueFees = overdueFees
                .Select(f => f.StudentId)
                .Distinct()
                .Count();

            // Calculate total overdue amount
            var totalOverdueAmount = overdueFees.Sum(f => f.AmountDue);

            // Get recent performances for parent's children (last 30 days)
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var recentPerformances = await _context.StudentPerformances
                .Where(p => childrenIds.Contains(p.StudentId) && p.ExamDate >= thirtyDaysAgo)
                .OrderByDescending(p => p.ExamDate)
                .Take(10)
                .Select(p => new
                {
                    id = p.Id.ToString(),
                    studentId = p.StudentId.ToString(),
                    studentFirstName = p.Student.FirstName,
                    studentLastName = p.Student.LastName,
                    subject = p.Subject,
                    examType = p.ExamType,
                    examDate = p.ExamDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    score = p.Score,
                    maxScore = p.MaxScore,
                    examTitle = p.ExamTitle,
                    comments = p.Comments,
                    percentage = p.MaxScore > 0 ? (decimal)p.Score / p.MaxScore * 100 : 0,
                    grade = p.Grade,
                    formattedScore = p.MaxScore > 0 ? $"{p.Score}/{p.MaxScore}" : p.Score.ToString(),
                    createdAt = p.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    updatedAt = p.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
                })
                .ToListAsync(cancellationToken);

            var metrics = new
            {
                totalChildren = totalChildren,
                childrenWithOverdueFees = childrenWithOverdueFees,
                totalOverdueAmount = totalOverdueAmount,
                recentPerformances = recentPerformances
            };

            _logger.LogInformation("Parent metrics retrieved successfully for ParentId: {ParentId}", parentId);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving parent metrics for ParentId: {ParentId}", parentId);
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving parent metrics." });
        }
    }

    /// <summary>
    /// Get admin metrics for dashboard
    /// </summary>
    /// <remarks>
    /// Retrieves comprehensive system metrics for the admin dashboard including:
    /// - Student counts (total, active, inactive)
    /// - Faculty counts (total, active, inactive)
    /// - Parent counts
    /// - Overdue payment information
    /// - Recent enrollment statistics
    /// 
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "totalStudents": 150,
    ///   "totalFaculty": 25,
    ///   "totalParents": 120,
    ///   "activeStudents": 145,
    ///   "inactiveStudents": 5,
    ///   "activeFaculty": 24,
    ///   "inactiveFaculty": 1,
    ///   "overduePayments": 12,
    ///   "totalOverdueAmount": 0,
    ///   "recentEnrollments": 8
    /// }
    /// ```
    /// </remarks>
    /// <returns>Admin metrics data</returns>
    /// <response code="200">Metrics retrieved successfully.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin role required.</response>
    /// <response code="500">Internal server error during metrics retrieval.</response>
    [HttpGet("admin")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 403)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    public async Task<ActionResult> GetAdminMetrics(CancellationToken cancellationToken)
    {
        try
        {
            var totalStudents = await _context.Students.CountAsync(cancellationToken);
            var activeStudents = await _context.Students.CountAsync(s => s.Status == StudentStatus.Active, cancellationToken);
            var inactiveStudents = totalStudents - activeStudents;

            var totalFaculty = await _context.Faculty.CountAsync(cancellationToken);
            var activeFaculty = await _context.Faculty.CountAsync(f => f.IsActive, cancellationToken);
            var inactiveFaculty = totalFaculty - activeFaculty;

            var totalParents = await _context.Parents.CountAsync(cancellationToken);

            // Recent enrollments (last 30 days)
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var recentEnrollments = await _context.Students
                .CountAsync(s => s.EnrollmentDate <= thirtyDaysAgo, cancellationToken);

            // Overdue payments count
            var overduePayments = await _context.StudentFees
                .CountAsync(f => f.DueDate < DateTime.UtcNow && f.PaymentStatus != PaymentStatus.Paid, cancellationToken);
            
            // Total overdue amount - set to 0 for now due to encryption handling
            var totalOverdueAmount = 0m;

            var metrics = new
            {
                TotalStudents = totalStudents,
                TotalFaculty = totalFaculty,
                TotalParents = totalParents,
                ActiveStudents = activeStudents,
                InactiveStudents = inactiveStudents,
                ActiveFaculty = activeFaculty,
                InactiveFaculty = inactiveFaculty,
                OverduePayments = overduePayments,
                TotalOverdueAmount = totalOverdueAmount,
                RecentEnrollments = recentEnrollments
            };

            _logger.LogInformation("Admin metrics retrieved successfully");
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving admin metrics");
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving admin metrics." });
        }
    }
}