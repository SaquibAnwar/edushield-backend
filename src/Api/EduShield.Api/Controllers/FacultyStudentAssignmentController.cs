using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EduShield.Core.Dtos;
using EduShield.Core.Interfaces;
using EduShield.Core.Enums;
using EduShield.Api.Auth;

namespace EduShield.Api.Controllers;

/// <summary>
/// Controller for managing faculty-student assignments
/// </summary>
[ApiController]
[Route("api/v1/faculty-student-assignments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), 400)]
[ProducesResponseType(typeof(ProblemDetails), 401)]
[ProducesResponseType(typeof(ProblemDetails), 403)]
[ProducesResponseType(typeof(ProblemDetails), 500)]
[Authorize]
public class FacultyStudentAssignmentController : ControllerBase
{
    private readonly IFacultyStudentAssignmentService _assignmentService;

    public FacultyStudentAssignmentController(IFacultyStudentAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    /// <summary>
    /// Assigns a student to a faculty member
    /// </summary>
    /// <param name="request">Assignment request</param>
    /// <returns>Assignment result</returns>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.AdminOrFaculty)]
    [ProducesResponseType(typeof(FacultyStudentAssignmentDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult<FacultyStudentAssignmentDto>> AssignStudentToFaculty([FromBody] CreateFacultyStudentAssignmentRequest request)
    {
        var result = await _assignmentService.AssignStudentToFacultyAsync(request);
        
        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(new { error = result.Message });
            
            return BadRequest(new { error = result.Message, errors = result.Errors });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Assigns multiple students to a faculty member
    /// </summary>
    /// <param name="request">Bulk assignment request</param>
    /// <returns>Bulk assignment result</returns>
    [HttpPost("bulk")]
    [Authorize(Policy = AuthorizationPolicies.AdminOrFaculty)]
    [ProducesResponseType(typeof(List<FacultyStudentAssignmentDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<ActionResult<List<FacultyStudentAssignmentDto>>> BulkAssignStudentsToFaculty([FromBody] BulkFacultyStudentAssignmentRequest request)
    {
        var result = await _assignmentService.BulkAssignStudentsToFacultyAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(new { error = result.Message, errors = result.Errors });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Updates a faculty-student assignment
    /// </summary>
    /// <param name="request">Update request</param>
    /// <returns>Updated assignment</returns>
    [HttpPut]
    [Authorize(Policy = AuthorizationPolicies.AdminOrFaculty)]
    [ProducesResponseType(typeof(FacultyStudentAssignmentDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult<FacultyStudentAssignmentDto>> UpdateAssignment([FromBody] UpdateFacultyStudentAssignmentRequest request)
    {
        var result = await _assignmentService.UpdateAssignmentAsync(request);
        
        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(new { error = result.Message });
            
            return BadRequest(new { error = result.Message, errors = result.Errors });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Deactivates a faculty-student assignment
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>Deactivation result</returns>
    [HttpDelete("{facultyId}/{studentId}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOrFaculty)]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult> DeactivateAssignment(Guid facultyId, Guid studentId)
    {
        var result = await _assignmentService.DeactivateAssignmentAsync(facultyId, studentId);
        
        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(new { error = result.Message });
            
            return BadRequest(new { error = result.Message, errors = result.Errors });
        }

        return Ok(new { message = "Assignment deactivated successfully" });
    }

    /// <summary>
    /// Activates a faculty-student assignment
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>Activation result</returns>
    [HttpPatch("{facultyId}/{studentId}/activate")]
    [Authorize(Policy = AuthorizationPolicies.AdminOrFaculty)]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult> ActivateAssignment(Guid facultyId, Guid studentId)
    {
        var result = await _assignmentService.ActivateAssignmentAsync(facultyId, studentId);
        
        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(new { error = result.Message });
            
            return BadRequest(new { error = result.Message, errors = result.Errors });
        }

        return Ok(new { message = "Assignment activated successfully" });
    }

    /// <summary>
    /// Gets a specific faculty-student assignment
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>Assignment if found</returns>
    [HttpGet("{facultyId}/{studentId}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOrFaculty)]
    [ProducesResponseType(typeof(FacultyStudentAssignmentDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult<FacultyStudentAssignmentDto>> GetAssignment(Guid facultyId, Guid studentId)
    {
        var result = await _assignmentService.GetAssignmentAsync(facultyId, studentId);
        
        if (!result.Success)
        {
            return BadRequest(new { error = result.Message, errors = result.Errors });
        }

        if (result.Data == null)
        {
            return NotFound(new { error = "Assignment not found" });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Gets all assignments for a faculty member
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <returns>List of assignments</returns>
    [HttpGet("faculty/{facultyId}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOrFaculty)]
    [ProducesResponseType(typeof(List<FacultyStudentAssignmentDto>), 200)]
    public async Task<ActionResult<List<FacultyStudentAssignmentDto>>> GetFacultyAssignments(Guid facultyId)
    {
        var result = await _assignmentService.GetFacultyAssignmentsAsync(facultyId);
        
        if (!result.Success)
        {
            return BadRequest(new { error = result.Message, errors = result.Errors });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Gets all assignments for a student
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <returns>List of assignments</returns>
    [HttpGet("student/{studentId}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOrFaculty)]
    [ProducesResponseType(typeof(List<FacultyStudentAssignmentDto>), 200)]
    public async Task<ActionResult<List<FacultyStudentAssignmentDto>>> GetStudentAssignments(Guid studentId)
    {
        var result = await _assignmentService.GetStudentAssignmentsAsync(studentId);
        
        if (!result.Success)
        {
            return BadRequest(new { error = result.Message, errors = result.Errors });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Gets faculty-student assignments with filtering and pagination
    /// </summary>
    /// <param name="filter">Filter criteria</param>
    /// <returns>Paginated list of assignments</returns>
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.AdminOrFaculty)]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<ActionResult> GetAssignments([FromQuery] FacultyStudentAssignmentFilterDto filter)
    {
        var result = await _assignmentService.GetAssignmentsAsync(filter);
        
        if (!result.Success)
        {
            return BadRequest(new { error = result.Message, errors = result.Errors });
        }

        var (assignments, totalCount) = result.Data;
        
        return Ok(new
        {
            assignments,
            totalCount,
            page = filter.Page,
            pageSize = filter.PageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
        });
    }

    /// <summary>
    /// Gets faculty dashboard with assigned students
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <returns>Faculty dashboard data</returns>
    [HttpGet("faculty/{facultyId}/dashboard")]
    [Authorize(Policy = AuthorizationPolicies.AdminOrFaculty)]
    [ProducesResponseType(typeof(FacultyDashboardDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult<FacultyDashboardDto>> GetFacultyDashboard(Guid facultyId)
    {
        var result = await _assignmentService.GetFacultyDashboardAsync(facultyId);
        
        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
                return NotFound(new { error = result.Message });
            
            return BadRequest(new { error = result.Message, errors = result.Errors });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Checks if a student is assigned to a faculty member
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>True if assigned, false otherwise</returns>
    [HttpGet("{facultyId}/{studentId}/exists")]
    [Authorize(Policy = AuthorizationPolicies.AdminOrFaculty)]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<ActionResult<bool>> IsStudentAssignedToFaculty(Guid facultyId, Guid studentId)
    {
        var result = await _assignmentService.IsStudentAssignedToFacultyAsync(facultyId, studentId);
        
        if (!result.Success)
        {
            return BadRequest(new { error = result.Message, errors = result.Errors });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Gets the count of active assignments for a faculty
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <returns>Count of active assignments</returns>
    [HttpGet("faculty/{facultyId}/active-count")]
    [Authorize(Policy = AuthorizationPolicies.AdminOrFaculty)]
    [ProducesResponseType(typeof(int), 200)]
    public async Task<ActionResult<int>> GetFacultyActiveAssignmentCount(Guid facultyId)
    {
        var result = await _assignmentService.GetFacultyActiveAssignmentCountAsync(facultyId);
        
        if (!result.Success)
        {
            return BadRequest(new { error = result.Message, errors = result.Errors });
        }

        return Ok(result.Data);
    }
}
