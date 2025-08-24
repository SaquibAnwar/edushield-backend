using EduShield.Core.Dtos;
using EduShield.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduShield.Api.Controllers;

/// <summary>
/// Controller for managing faculty members
/// </summary>
[ApiController]
[Route("api/v1/faculty")]
[Authorize(Roles = "Admin,DevAuth")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), 400)]
[ProducesResponseType(typeof(ProblemDetails), 401)]
[ProducesResponseType(typeof(ProblemDetails), 403)]
[ProducesResponseType(typeof(ProblemDetails), 404)]
[ProducesResponseType(typeof(ProblemDetails), 500)]
public class FacultyController : ControllerBase
{
    private readonly IFacultyService _facultyService;
    private readonly ILogger<FacultyController> _logger;

    public FacultyController(IFacultyService facultyService, ILogger<FacultyController> logger)
    {
        _facultyService = facultyService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new faculty member
    /// </summary>
    /// <remarks>
    /// Creates a new faculty member with the provided information.
    /// The system will automatically generate a unique employee ID in the format "faculty_XXX".
    /// 
    /// **Required Fields:**
    /// - FirstName, LastName, Email, PhoneNumber, DateOfBirth, Address, Gender, Department, Subject, HireDate
    /// 
    /// **Validations:**
    /// - Email must be unique across all faculty members
    /// - Date of birth must be in the past and faculty must be at least 18 years old
    /// - Hire date must be in the past or present
    /// - UserId is optional but must reference an existing user if provided
    /// 
    /// **Sample Request:**
    /// ```json
    /// {
    ///   "firstName": "John",
    ///   "lastName": "Doe",
    ///   "email": "john.doe@university.edu",
    ///   "phoneNumber": "+1-555-123-4567",
    ///   "dateOfBirth": "1985-03-15",
    ///   "address": "123 University Ave, City, State 12345",
    ///   "gender": 0,
    ///   "department": "Computer Science",
    ///   "subject": "Software Engineering",
    ///   "hireDate": "2020-08-15"
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Faculty creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created faculty information</returns>
    /// <response code="201">Faculty created successfully. Returns the created faculty details.</response>
    /// <response code="400">Invalid request data or validation failed.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin or DevAuth role required.</response>
    /// <response code="500">Internal server error during faculty creation.</response>
    [HttpPost]
    [ProducesResponseType(typeof(FacultyDto), 201)]
    public async Task<ActionResult<FacultyDto>> CreateFaculty(
        [FromBody] CreateFacultyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var faculty = await _facultyService.CreateAsync(request, cancellationToken);
            _logger.LogInformation("Faculty created successfully with ID: {FacultyId}", faculty.Id);
            return CreatedAtAction(nameof(GetFaculty), new { id = faculty.Id }, faculty);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create faculty: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating faculty");
            return StatusCode(500, new { error = "An unexpected error occurred while creating the faculty member." });
        }
    }

    /// <summary>
    /// Get a specific faculty member by ID
    /// </summary>
    /// <remarks>
    /// Retrieves detailed information about a faculty member by their unique identifier.
    /// 
    /// **Returns:**
    /// - Basic personal information (name, email, phone, address, DOB, gender)
    /// - Academic information (department, subject, employee ID, hire date)
    /// - Status information (active/inactive, creation/update timestamps)
    /// - Computed properties (full name, age, years of service, employment status)
    /// 
    /// **Sample Response:**
    /// ```json
    /// {
    ///   "id": "443abd4f-9e56-4adc-9eb7-7a0e2522dd2b",
    ///   "firstName": "John",
    ///   "lastName": "Doe",
    ///   "email": "john.doe@university.edu",
    ///   "employeeId": "faculty_001",
    ///   "department": "Computer Science",
    ///   "subject": "Software Engineering",
    ///   "fullName": "John Doe",
    ///   "age": 38,
    ///   "yearsOfService": 3,
    ///   "isEmployed": true
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Faculty ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Faculty information</returns>
    /// <response code="200">Faculty found successfully. Returns faculty details.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin or DevAuth role required.</response>
    /// <response code="404">Faculty not found.</response>
    /// <response code="500">Internal server error during retrieval.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FacultyDto), 200)]
    public async Task<ActionResult<FacultyDto>> GetFaculty(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var faculty = await _facultyService.GetByIdAsync(id, cancellationToken);
            if (faculty == null)
            {
                return NotFound(new { error = "Faculty member not found." });
            }

            return Ok(faculty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving faculty with ID: {FacultyId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving the faculty member." });
        }
    }

    /// <summary>
    /// Get a faculty member by email
    /// </summary>
    /// <remarks>
    /// Retrieves faculty information by their email address.
    /// 
    /// **Note:** Email search is case-insensitive.
    /// </remarks>
    /// <param name="email">Faculty email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Faculty information</returns>
    /// <response code="200">Faculty found successfully. Returns faculty details.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin or DevAuth role required.</response>
    /// <response code="404">Faculty not found.</response>
    /// <response code="500">Internal server error during retrieval.</response>
    [HttpGet("email/{email}")]
    [ProducesResponseType(typeof(FacultyDto), 200)]
    public async Task<ActionResult<FacultyDto>> GetFacultyByEmail(
        string email,
        CancellationToken cancellationToken)
    {
        try
        {
            var faculty = await _facultyService.GetByEmailAsync(email, cancellationToken);
            if (faculty == null)
            {
                return NotFound(new { error = "Faculty member not found." });
            }

            return Ok(faculty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving faculty with email: {Email}", email);
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving the faculty member." });
        }
    }

    /// <summary>
    /// Get a faculty member by employee ID
    /// </summary>
    /// <remarks>
    /// Retrieves faculty information by their employee ID (e.g., "faculty_001").
    /// 
    /// **Employee ID Format:** faculty_XXX where XXX is a zero-padded 3-digit number
    /// </remarks>
    /// <param name="employeeId">Faculty employee ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Faculty information</returns>
    /// <response code="200">Faculty found successfully. Returns faculty details.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin or DevAuth role required.</response>
    /// <response code="404">Faculty not found.</response>
    /// <response code="500">Internal server error during retrieval.</response>
    [HttpGet("employee/{employeeId}")]
    [ProducesResponseType(typeof(FacultyDto), 200)]
    public async Task<ActionResult<FacultyDto>> GetFacultyByEmployeeId(
        string employeeId,
        CancellationToken cancellationToken)
    {
        try
        {
            var faculty = await _facultyService.GetByEmployeeIdAsync(employeeId, cancellationToken);
            if (faculty == null)
            {
                return NotFound(new { error = "Faculty member not found." });
            }

            return Ok(faculty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving faculty with employee ID: {EmployeeId}", employeeId);
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving the faculty member." });
        }
    }

    /// <summary>
    /// Get all faculty members
    /// </summary>
    /// <remarks>
    /// Retrieves a list of all faculty members in the system.
    /// Results are ordered by employee ID for consistent pagination.
    /// 
    /// **Returns:** Collection of faculty DTOs with all basic and computed information
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all faculty members</returns>
    /// <response code="200">Faculty list retrieved successfully.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin or DevAuth role required.</response>
    /// <response code="500">Internal server error during retrieval.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FacultyDto>), 200)]
    public async Task<ActionResult<IEnumerable<FacultyDto>>> GetAllFaculty(
        CancellationToken cancellationToken)
    {
        try
        {
            var faculty = await _facultyService.GetAllAsync(cancellationToken);
            return Ok(faculty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving faculty list");
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving faculty members." });
        }
    }

    /// <summary>
    /// Get faculty members by department
    /// </summary>
    /// <remarks>
    /// Retrieves all faculty members belonging to a specific department.
    /// Search is case-insensitive.
    /// 
    /// **Use Cases:**
    /// - Department overview
    /// - Staff planning
    /// - Resource allocation
    /// </remarks>
    /// <param name="department">Department name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of faculty members in the specified department</returns>
    /// <response code="200">Faculty list retrieved successfully.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin or DevAuth role required.</response>
    /// <response code="500">Internal server error during retrieval.</response>
    [HttpGet("department/{department}")]
    [ProducesResponseType(typeof(IEnumerable<FacultyDto>), 200)]
    public async Task<ActionResult<IEnumerable<FacultyDto>>> GetFacultyByDepartment(
        string department,
        CancellationToken cancellationToken)
    {
        try
        {
            var faculty = await _facultyService.GetByDepartmentAsync(department, cancellationToken);
            return Ok(faculty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving faculty for department: {Department}", department);
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving faculty members." });
        }
    }

    /// <summary>
    /// Get faculty members by subject
    /// </summary>
    /// <remarks>
    /// Retrieves all faculty members teaching a specific subject.
    /// Search is case-insensitive.
    /// 
    /// **Use Cases:**
    /// - Subject coverage analysis
    /// - Teaching assignment planning
    /// - Expertise identification
    /// </remarks>
    /// <param name="subject">Subject name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of faculty members teaching the specified subject</returns>
    /// <response code="200">Faculty list retrieved successfully.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin or DevAuth role required.</response>
    /// <response code="500">Internal server error during retrieval.</response>
    [HttpGet("subject/{subject}")]
    [ProducesResponseType(typeof(IEnumerable<FacultyDto>), 200)]
    public async Task<ActionResult<IEnumerable<FacultyDto>>> GetFacultyBySubject(
        string subject,
        CancellationToken cancellationToken)
    {
        try
        {
            var faculty = await _facultyService.GetBySubjectAsync(subject, cancellationToken);
            return Ok(faculty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving faculty for subject: {Subject}", subject);
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving faculty members." });
        }
    }

    /// <summary>
    /// Get active faculty members
    /// </summary>
    /// <remarks>
    /// Retrieves only active faculty members (IsActive = true).
    /// Useful for current staff listings and active teaching assignments.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active faculty members</returns>
    /// <response code="200">Active faculty list retrieved successfully.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin or DevAuth role required.</response>
    /// <response code="500">Internal server error during retrieval.</response>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<FacultyDto>), 200)]
    public async Task<ActionResult<IEnumerable<FacultyDto>>> GetActiveFaculty(
        CancellationToken cancellationToken)
    {
        try
        {
            var faculty = await _facultyService.GetActiveAsync(cancellationToken);
            return Ok(faculty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving active faculty list");
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving active faculty members." });
        }
    }

    /// <summary>
    /// Update a faculty member
    /// </summary>
    /// <remarks>
    /// Updates an existing faculty member's information.
    /// Only provided fields will be updated; omitted fields remain unchanged.
    /// 
    /// **Validations:**
    /// - Email uniqueness (if changing)
    /// - Date validations (DOB in past, hire date not in future)
    /// - Minimum age requirement (18 years)
    /// - User ID existence (if changing)
    /// 
    /// **Sample Request:**
    /// ```json
    /// {
    ///   "department": "Computer Engineering",
    ///   "subject": "Computer Architecture",
    ///   "isActive": true
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Faculty ID to update</param>
    /// <param name="request">Faculty update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated faculty information</returns>
    /// <response code="200">Faculty updated successfully. Returns updated faculty details.</response>
    /// <response code="400">Invalid request data or validation failed.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin or DevAuth role required.</response>
    /// <response code="404">Faculty not found.</response>
    /// <response code="500">Internal server error during update.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(FacultyDto), 200)]
    public async Task<ActionResult<FacultyDto>> UpdateFaculty(
        Guid id,
        [FromBody] UpdateFacultyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var faculty = await _facultyService.UpdateAsync(id, request, cancellationToken);
            _logger.LogInformation("Faculty updated successfully with ID: {FacultyId}", id);
            return Ok(faculty);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update faculty {FacultyId}: {Message}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating faculty {FacultyId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred while updating the faculty member." });
        }
    }

    /// <summary>
    /// Delete a faculty member
    /// </summary>
    /// <remarks>
    /// Permanently removes a faculty member from the system.
    /// 
    /// **Warning:** This action cannot be undone and will remove all associated data.
    /// Consider deactivating the faculty member instead by setting IsActive to false.
    /// </remarks>
    /// <param name="id">Faculty ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">Faculty deleted successfully.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin or DevAuth role required.</response>
    /// <response code="404">Faculty not found.</response>
    /// <response code="500">Internal server error during deletion.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    public async Task<ActionResult> DeleteFaculty(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!await _facultyService.ExistsAsync(id, cancellationToken))
            {
                return NotFound(new { error = "Faculty member not found." });
            }

            await _facultyService.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("Faculty deleted successfully with ID: {FacultyId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting faculty {FacultyId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred while deleting the faculty member." });
        }
    }

    /// <summary>
    /// Check if a faculty member exists
    /// </summary>
    /// <remarks>
    /// Checks whether a faculty member with the specified ID exists in the system.
    /// Returns a boolean value indicating existence.
    /// </remarks>
    /// <param name="id">Faculty ID to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if faculty exists, false otherwise</returns>
    /// <response code="200">Existence check completed successfully.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin or DevAuth role required.</response>
    /// <response code="500">Internal server error during check.</response>
    [HttpGet("{id:guid}/exists")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<ActionResult<bool>> FacultyExists(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var exists = await _facultyService.ExistsAsync(id, cancellationToken);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if faculty exists: {FacultyId}", id);
            return StatusCode(500, new { error = "An unexpected error occurred while checking faculty existence." });
        }
    }

    /// <summary>
    /// Check if an email is already taken
    /// </summary>
    /// <remarks>
    /// Checks whether an email address is already registered by another faculty member.
    /// Useful for form validation before submission.
    /// </remarks>
    /// <param name="email">Email address to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if email exists, false otherwise</returns>
    /// <response code="200">Email check completed successfully.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin or DevAuth role required.</response>
    /// <response code="500">Internal server error during check.</response>
    [HttpGet("email/{email}/exists")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<ActionResult<bool>> EmailExists(
        string email,
        CancellationToken cancellationToken)
    {
        try
        {
            var exists = await _facultyService.EmailExistsAsync(email, cancellationToken);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if email exists: {Email}", email);
            return StatusCode(500, new { error = "An unexpected error occurred while checking email existence." });
        }
    }

    /// <summary>
    /// Check if an employee ID is already taken
    /// </summary>
    /// <remarks>
    /// Checks whether an employee ID is already assigned to another faculty member.
    /// Useful for form validation before submission.
    /// </remarks>
    /// <param name="employeeId">Employee ID to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if employee ID exists, false otherwise</returns>
    /// <response code="200">Employee ID check completed successfully.</response>
    /// <response code="401">Unauthorized. Valid JWT token required.</response>
    /// <response code="403">Forbidden. Admin or DevAuth role required.</response>
    /// <response code="500">Internal server error during check.</response>
    [HttpGet("employee/{employeeId}/exists")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<ActionResult<bool>> EmployeeIdExists(
        string employeeId,
        CancellationToken cancellationToken)
    {
        try
        {
            var exists = await _facultyService.EmployeeIdExistsAsync(employeeId, cancellationToken);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if employee ID exists: {EmployeeId}", employeeId);
            return StatusCode(500, new { error = "An unexpected error occurred while checking employee ID existence." });
        }
    }
}
