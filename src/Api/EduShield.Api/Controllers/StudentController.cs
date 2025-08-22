using EduShield.Api.Auth.Requirements;
using EduShield.Core.Dtos;
using EduShield.Core.Enums;
using EduShield.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduShield.Api.Controllers;

[ApiController]
[Route("api/v1/students")]
[Authorize]
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly ILogger<StudentController> _logger;

    public StudentController(IStudentService studentService, ILogger<StudentController> logger)
    {
        _studentService = studentService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new student (Admin/Dev only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,DevAuth")]
    public async Task<ActionResult<StudentDto>> CreateStudent(
        [FromBody] CreateStudentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var student = await _studentService.CreateAsync(request, cancellationToken);
            _logger.LogInformation("Student created successfully with ID: {StudentId}", student.Id);
            return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create student: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating student");
            return StatusCode(500, new { error = "An unexpected error occurred while creating the student." });
        }
    }

    /// <summary>
    /// Get a specific student by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "StudentAccess")]
    public async Task<ActionResult<StudentDto>> GetStudent(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var student = await _studentService.GetByIdAsync(id, cancellationToken);
            if (student == null)
            {
                return NotFound(new { error = "Student not found." });
            }

            return Ok(student);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving student with ID: {StudentId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving the student." });
        }
    }

    /// <summary>
    /// Get a student by email
    /// </summary>
    [HttpGet("email/{email}")]
    [Authorize(Policy = "StudentAccess")]
    public async Task<ActionResult<StudentDto>> GetStudentByEmail(
        string email,
        CancellationToken cancellationToken)
    {
        try
        {
            var student = await _studentService.GetByEmailAsync(email, cancellationToken);
            if (student == null)
            {
                return NotFound(new { error = "Student not found." });
            }

            return Ok(student);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving student with email: {Email}", email);
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving the student." });
        }
    }

    /// <summary>
    /// Get a student by roll number
    /// </summary>
    [HttpGet("rollnumber/{rollNumber}")]
    [Authorize(Policy = "StudentAccess")]
    public async Task<ActionResult<StudentDto>> GetStudentByRollNumber(
        string rollNumber,
        CancellationToken cancellationToken)
    {
        try
        {
            var student = await _studentService.GetByRollNumberAsync(rollNumber, cancellationToken);
            if (student == null)
            {
                return NotFound(new { error = "Student not found." });
            }

            return Ok(student);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving student with roll number: {RollNumber}", rollNumber);
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving the student." });
        }
    }

    /// <summary>
    /// Get all students (Admin/Dev have full access, others see filtered data)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StudentDto>>> GetAllStudents(
        CancellationToken cancellationToken)
    {
        try
        {
            var userRole = GetCurrentUserRole();
            var userId = GetCurrentUserId();

            IEnumerable<StudentDto> students;

            switch (userRole)
            {
                case UserRole.Admin:
                case UserRole.DevAuth:
                    // Admin/Dev can see all students
                    students = await _studentService.GetAllAsync(cancellationToken);
                    break;

                case UserRole.Faculty:
                    // Faculty can only see students assigned to them
                    if (userId.HasValue)
                    {
                        students = await _studentService.GetByFacultyIdAsync(userId.Value, cancellationToken);
                    }
                    else
                    {
                        return Unauthorized(new { error = "User ID not found." });
                    }
                    break;

                case UserRole.Parent:
                    // Parent can only see their children
                    if (userId.HasValue)
                    {
                        students = await _studentService.GetByParentIdAsync(userId.Value, cancellationToken);
                    }
                    else
                    {
                        return Unauthorized(new { error = "User ID not found." });
                    }
                    break;

                case UserRole.Student:
                    // Students cannot see other students
                    return Forbid();

                default:
                    return Forbid();
            }

            return Ok(students);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving students");
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving students." });
        }
    }

    /// <summary>
    /// Get students by faculty ID
    /// </summary>
    [HttpGet("faculty/{facultyId:guid}")]
    [Authorize(Roles = "Admin,DevAuth,Faculty")]
    public async Task<ActionResult<IEnumerable<StudentDto>>> GetStudentsByFaculty(
        Guid facultyId,
        CancellationToken cancellationToken)
    {
        try
        {
            var students = await _studentService.GetByFacultyIdAsync(facultyId, cancellationToken);
            return Ok(students);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving students for faculty: {FacultyId}", facultyId);
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving students." });
        }
    }

    /// <summary>
    /// Get students by parent ID
    /// </summary>
    [HttpGet("parent/{parentId:guid}")]
    [Authorize(Roles = "Admin,DevAuth,Parent")]
    public async Task<ActionResult<IEnumerable<StudentDto>>> GetStudentsByParent(
        Guid parentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var students = await _studentService.GetByParentIdAsync(parentId, cancellationToken);
            return Ok(students);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving students for parent: {ParentId}", parentId);
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving students." });
        }
    }

    /// <summary>
    /// Get students by status
    /// </summary>
    [HttpGet("status/{status}")]
    [Authorize(Roles = "Admin,DevAuth,Faculty")]
    public async Task<ActionResult<IEnumerable<StudentDto>>> GetStudentsByStatus(
        StudentStatus status,
        CancellationToken cancellationToken)
    {
        try
        {
            var students = await _studentService.GetByStatusAsync(status, cancellationToken);
            return Ok(students);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving students with status: {Status}", status);
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving students." });
        }
    }

    /// <summary>
    /// Update a student (Admin/Dev only)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,DevAuth")]
    public async Task<ActionResult<StudentDto>> UpdateStudent(
        Guid id,
        [FromBody] UpdateStudentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var student = await _studentService.UpdateAsync(id, request, cancellationToken);
            _logger.LogInformation("Student updated successfully with ID: {StudentId}", id);
            return Ok(student);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update student {StudentId}: {Message}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating student {StudentId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred while updating the student." });
        }
    }

    /// <summary>
    /// Delete a student (Admin/Dev only)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,DevAuth")]
    public async Task<ActionResult> DeleteStudent(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!await _studentService.ExistsAsync(id, cancellationToken))
            {
                return NotFound(new { error = "Student not found." });
            }

            await _studentService.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("Student deleted successfully with ID: {StudentId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting student {StudentId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred while deleting the student." });
        }
    }

    /// <summary>
    /// Check if a student exists
    /// </summary>
    [HttpGet("{id:guid}/exists")]
    [Authorize(Roles = "Admin,DevAuth")]
    public async Task<ActionResult<bool>> StudentExists(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var exists = await _studentService.ExistsAsync(id, cancellationToken);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if student exists: {StudentId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred while checking student existence." });
        }
    }

    /// <summary>
    /// Check if an email is already taken
    /// </summary>
    [HttpGet("email/{email}/exists")]
    [Authorize(Roles = "Admin,DevAuth")]
    public async Task<ActionResult<bool>> EmailExists(
        string email,
        CancellationToken cancellationToken)
    {
        try
        {
            var exists = await _studentService.EmailExistsAsync(email, cancellationToken);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if email exists: {Email}", email);
            return StatusCode(500, new { error = "An unexpected error occurred while checking email existence." });
        }
    }

    /// <summary>
    /// Check if a roll number is already taken
    /// </summary>
    [HttpGet("rollnumber/{rollNumber}/exists")]
    [Authorize(Roles = "Admin,DevAuth")]
    public async Task<ActionResult<bool>> RollNumberExists(
        string rollNumber,
        CancellationToken cancellationToken)
    {
        try
        {
            var exists = await _studentService.RollNumberExistsAsync(rollNumber, cancellationToken);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if roll number exists: {RollNumber}", rollNumber);
            return StatusCode(500, new { error = "An unexpected error occurred while checking roll number existence." });
        }
    }

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
}
