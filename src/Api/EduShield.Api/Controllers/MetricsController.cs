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
                .CountAsync(s => s.EnrollmentDate >= thirtyDaysAgo, cancellationToken);

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