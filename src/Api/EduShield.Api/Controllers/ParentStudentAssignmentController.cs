using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EduShield.Core.Dtos;
using EduShield.Core.Interfaces;
using EduShield.Api.Auth.Requirements;
using System.ComponentModel.DataAnnotations;

namespace EduShield.Api.Controllers;

/// <summary>
/// Controller for managing parent-student assignments
/// Only Admin and DevAuth roles can manage these assignments
/// </summary>
[ApiController]
[Route("api/v1/parent-student-assignments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), 400)]
[ProducesResponseType(typeof(ProblemDetails), 401)]
[ProducesResponseType(typeof(ProblemDetails), 403)]
[ProducesResponseType(typeof(ProblemDetails), 404)]
[ProducesResponseType(typeof(ProblemDetails), 500)]
[Authorize(Policy = "ParentStudentAssignmentPolicy")]
public class ParentStudentAssignmentController : ControllerBase
{
    private readonly IParentStudentAssignmentService _assignmentService;
    private readonly ILogger<ParentStudentAssignmentController> _logger;

    public ParentStudentAssignmentController(
        IParentStudentAssignmentService assignmentService,
        ILogger<ParentStudentAssignmentController> logger)
    {
        _assignmentService = assignmentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all parent-student assignments
    /// </summary>
    /// <returns>List of all parent-student assignments</returns>
    /// <response code="200">Returns all parent-student assignments</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ParentStudentAssignmentDto>), 200)]
    public async Task<ActionResult<IEnumerable<ParentStudentAssignmentDto>>> GetAllAssignments()
    {
        try
        {
            var assignments = await _assignmentService.GetAllAssignmentsAsync();
            _logger.LogInformation("Retrieved {Count} parent-student assignments", assignments.Count());
            return Ok(assignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all parent-student assignments");
            return StatusCode(500, new { error = "An error occurred while retrieving assignments" });
        }
    }

    /// <summary>
    /// Get a specific parent-student assignment
    /// </summary>
    /// <param name="parentId">Parent ID</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>Parent-student assignment details</returns>
    /// <response code="200">Returns the assignment details</response>
    /// <response code="404">Assignment not found</response>
    [HttpGet("{parentId:guid}/{studentId:guid}")]
    [ProducesResponseType(typeof(ParentStudentAssignmentDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ParentStudentAssignmentDto>> GetAssignment(
        [FromRoute] Guid parentId,
        [FromRoute] Guid studentId)
    {
        try
        {
            var assignment = await _assignmentService.GetAssignmentAsync(parentId, studentId);
            
            if (assignment == null)
            {
                _logger.LogWarning("Assignment not found for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
                return NotFound(new { error = "Assignment not found" });
            }

            return Ok(assignment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignment for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            return StatusCode(500, new { error = "An error occurred while retrieving the assignment" });
        }
    }

    /// <summary>
    /// Create a new parent-student assignment
    /// </summary>
    /// <param name="createDto">Assignment creation details</param>
    /// <returns>Created assignment details</returns>
    /// <response code="201">Assignment created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="409">Assignment already exists</response>
    [HttpPost]
    [ProducesResponseType(typeof(ParentStudentAssignmentDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<ParentStudentAssignmentDto>> CreateAssignment(
        [FromBody] CreateParentStudentAssignmentDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var assignment = await _assignmentService.CreateAssignmentAsync(createDto);
            
            _logger.LogInformation("Created assignment between ParentId: {ParentId} and StudentId: {StudentId} with relationship: {Relationship}", 
                createDto.ParentId, createDto.StudentId, createDto.Relationship);

            return CreatedAtAction(
                nameof(GetAssignment),
                new { parentId = assignment.ParentId, studentId = assignment.StudentId },
                assignment);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for creating assignment");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Assignment already exists");
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating assignment for ParentId: {ParentId}, StudentId: {StudentId}", 
                createDto.ParentId, createDto.StudentId);
            return StatusCode(500, new { error = "An error occurred while creating the assignment" });
        }
    }

    /// <summary>
    /// Update an existing parent-student assignment
    /// </summary>
    /// <param name="parentId">Parent ID</param>
    /// <param name="studentId">Student ID</param>
    /// <param name="updateDto">Assignment update details</param>
    /// <returns>Updated assignment details</returns>
    /// <response code="200">Assignment updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Assignment not found</response>
    [HttpPut("{parentId:guid}/{studentId:guid}")]
    [ProducesResponseType(typeof(ParentStudentAssignmentDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ParentStudentAssignmentDto>> UpdateAssignment(
        [FromRoute] Guid parentId,
        [FromRoute] Guid studentId,
        [FromBody] UpdateParentStudentAssignmentDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var assignment = await _assignmentService.UpdateAssignmentAsync(parentId, studentId, updateDto);
            
            _logger.LogInformation("Updated assignment for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            return Ok(assignment);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Assignment not found for update");
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating assignment for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            return StatusCode(500, new { error = "An error occurred while updating the assignment" });
        }
    }

    /// <summary>
    /// Delete a parent-student assignment
    /// </summary>
    /// <param name="parentId">Parent ID</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">Assignment deleted successfully</response>
    /// <response code="404">Assignment not found</response>
    [HttpDelete("{parentId:guid}/{studentId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteAssignment(
        [FromRoute] Guid parentId,
        [FromRoute] Guid studentId)
    {
        try
        {
            var result = await _assignmentService.DeleteAssignmentAsync(parentId, studentId);
            
            if (!result)
            {
                _logger.LogWarning("Assignment not found for deletion: ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
                return NotFound(new { error = "Assignment not found" });
            }

            _logger.LogInformation("Deleted assignment for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting assignment for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            return StatusCode(500, new { error = "An error occurred while deleting the assignment" });
        }
    }

    /// <summary>
    /// Get all assignments for a specific parent
    /// </summary>
    /// <param name="parentId">Parent ID</param>
    /// <param name="activeOnly">Return only active assignments</param>
    /// <returns>List of assignments for the parent</returns>
    /// <response code="200">Returns parent's assignments</response>
    [HttpGet("parent/{parentId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ParentStudentAssignmentDto>), 200)]
    public async Task<ActionResult<IEnumerable<ParentStudentAssignmentDto>>> GetAssignmentsByParent(
        [FromRoute] Guid parentId,
        [FromQuery] bool activeOnly = false)
    {
        try
        {
            var assignments = activeOnly 
                ? await _assignmentService.GetActiveAssignmentsByParentIdAsync(parentId)
                : await _assignmentService.GetAssignmentsByParentIdAsync(parentId);

            _logger.LogInformation("Retrieved {Count} assignments for ParentId: {ParentId} (activeOnly: {ActiveOnly})", 
                assignments.Count(), parentId, activeOnly);
            
            return Ok(assignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignments for ParentId: {ParentId}", parentId);
            return StatusCode(500, new { error = "An error occurred while retrieving parent assignments" });
        }
    }

    /// <summary>
    /// Get parent with all assigned students
    /// </summary>
    /// <param name="parentId">Parent ID</param>
    /// <returns>Parent details with assigned students</returns>
    /// <response code="200">Returns parent with students</response>
    /// <response code="404">Parent not found</response>
    [HttpGet("parent/{parentId:guid}/with-students")]
    [ProducesResponseType(typeof(ParentWithStudentsDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ParentWithStudentsDto>> GetParentWithStudents([FromRoute] Guid parentId)
    {
        try
        {
            var parentWithStudents = await _assignmentService.GetParentWithStudentsAsync(parentId);
            
            if (parentWithStudents == null)
            {
                _logger.LogWarning("Parent not found: {ParentId}", parentId);
                return NotFound(new { error = "Parent not found" });
            }

            return Ok(parentWithStudents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parent with students for ParentId: {ParentId}", parentId);
            return StatusCode(500, new { error = "An error occurred while retrieving parent with students" });
        }
    }

    /// <summary>
    /// Get all assignments for a specific student
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="activeOnly">Return only active assignments</param>
    /// <returns>List of assignments for the student</returns>
    /// <response code="200">Returns student's assignments</response>
    [HttpGet("student/{studentId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ParentStudentAssignmentDto>), 200)]
    public async Task<ActionResult<IEnumerable<ParentStudentAssignmentDto>>> GetAssignmentsByStudent(
        [FromRoute] Guid studentId,
        [FromQuery] bool activeOnly = false)
    {
        try
        {
            var assignments = activeOnly 
                ? await _assignmentService.GetActiveAssignmentsByStudentIdAsync(studentId)
                : await _assignmentService.GetAssignmentsByStudentIdAsync(studentId);

            _logger.LogInformation("Retrieved {Count} assignments for StudentId: {StudentId} (activeOnly: {ActiveOnly})", 
                assignments.Count(), studentId, activeOnly);
            
            return Ok(assignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignments for StudentId: {StudentId}", studentId);
            return StatusCode(500, new { error = "An error occurred while retrieving student assignments" });
        }
    }

    /// <summary>
    /// Get student with all assigned parents
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <returns>Student details with assigned parents</returns>
    /// <response code="200">Returns student with parents</response>
    /// <response code="404">Student not found</response>
    [HttpGet("student/{studentId:guid}/with-parents")]
    [ProducesResponseType(typeof(StudentWithParentsDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<StudentWithParentsDto>> GetStudentWithParents([FromRoute] Guid studentId)
    {
        try
        {
            var studentWithParents = await _assignmentService.GetStudentWithParentsAsync(studentId);
            
            if (studentWithParents == null)
            {
                _logger.LogWarning("Student not found: {StudentId}", studentId);
                return NotFound(new { error = "Student not found" });
            }

            return Ok(studentWithParents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student with parents for StudentId: {StudentId}", studentId);
            return StatusCode(500, new { error = "An error occurred while retrieving student with parents" });
        }
    }

    /// <summary>
    /// Set a parent as primary contact for a student
    /// </summary>
    /// <param name="parentId">Parent ID</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>Success status</returns>
    /// <response code="200">Primary contact set successfully</response>
    /// <response code="404">Assignment not found</response>
    [HttpPost("{parentId:guid}/{studentId:guid}/set-primary")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SetPrimaryContact(
        [FromRoute] Guid parentId,
        [FromRoute] Guid studentId)
    {
        try
        {
            var result = await _assignmentService.SetPrimaryContactAsync(parentId, studentId);
            
            if (!result)
            {
                _logger.LogWarning("Assignment not found for setting primary contact: ParentId: {ParentId}, StudentId: {StudentId}", 
                    parentId, studentId);
                return NotFound(new { error = "Assignment not found" });
            }

            _logger.LogInformation("Set primary contact: ParentId: {ParentId} for StudentId: {StudentId}", parentId, studentId);
            return Ok(new { message = "Primary contact set successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting primary contact for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            return StatusCode(500, new { error = "An error occurred while setting primary contact" });
        }
    }

    /// <summary>
    /// Remove primary contact status from a parent for a student
    /// </summary>
    /// <param name="parentId">Parent ID</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>Success status</returns>
    /// <response code="200">Primary contact removed successfully</response>
    /// <response code="404">Assignment not found</response>
    [HttpPost("{parentId:guid}/{studentId:guid}/remove-primary")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemovePrimaryContact(
        [FromRoute] Guid parentId,
        [FromRoute] Guid studentId)
    {
        try
        {
            var result = await _assignmentService.RemovePrimaryContactAsync(parentId, studentId);
            
            if (!result)
            {
                _logger.LogWarning("Assignment not found for removing primary contact: ParentId: {ParentId}, StudentId: {StudentId}", 
                    parentId, studentId);
                return NotFound(new { error = "Assignment not found" });
            }

            _logger.LogInformation("Removed primary contact: ParentId: {ParentId} for StudentId: {StudentId}", parentId, studentId);
            return Ok(new { message = "Primary contact removed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing primary contact for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            return StatusCode(500, new { error = "An error occurred while removing primary contact" });
        }
    }

    /// <summary>
    /// Create bulk assignments for a parent to multiple students
    /// </summary>
    /// <param name="bulkDto">Bulk assignment details</param>
    /// <returns>List of created assignments</returns>
    /// <response code="201">Assignments created successfully</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost("bulk")]
    [ProducesResponseType(typeof(IEnumerable<ParentStudentAssignmentDto>), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<IEnumerable<ParentStudentAssignmentDto>>> CreateBulkAssignments(
        [FromBody] BulkParentStudentAssignmentDto bulkDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var assignments = await _assignmentService.CreateBulkAssignmentsAsync(bulkDto);
            
            _logger.LogInformation("Created {Count} bulk assignments for ParentId: {ParentId}", 
                assignments.Count(), bulkDto.ParentId);

            return CreatedAtAction(nameof(GetAssignmentsByParent), new { parentId = bulkDto.ParentId }, assignments);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for creating bulk assignments");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation for bulk assignments");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bulk assignments for ParentId: {ParentId}", bulkDto.ParentId);
            return StatusCode(500, new { error = "An error occurred while creating bulk assignments" });
        }
    }

    /// <summary>
    /// Get assignment statistics
    /// </summary>
    /// <returns>Assignment statistics</returns>
    /// <response code="200">Returns assignment statistics</response>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<ActionResult> GetStatistics()
    {
        try
        {
            var totalCount = await _assignmentService.GetTotalAssignmentsCountAsync();
            var activeCount = await _assignmentService.GetActiveAssignmentsCountAsync();
            var relationshipTypes = await _assignmentService.GetAssignmentsByRelationshipTypeAsync();

            var statistics = new
            {
                TotalAssignments = totalCount,
                ActiveAssignments = activeCount,
                InactiveAssignments = totalCount - activeCount,
                RelationshipTypes = relationshipTypes
            };

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignment statistics");
            return StatusCode(500, new { error = "An error occurred while retrieving statistics" });
        }
    }

    /// <summary>
    /// Validate if a parent can be assigned to a student
    /// </summary>
    /// <param name="parentId">Parent ID</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>Validation result</returns>
    /// <response code="200">Returns validation result</response>
    [HttpGet("validate/{parentId:guid}/{studentId:guid}")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<ActionResult> ValidateAssignment(
        [FromRoute] Guid parentId,
        [FromRoute] Guid studentId)
    {
        try
        {
            var canAssign = await _assignmentService.CanAssignParentToStudentAsync(parentId, studentId);
            var isAssigned = await _assignmentService.IsParentAssignedToStudentAsync(parentId, studentId);

            var validation = new
            {
                CanAssign = canAssign,
                IsCurrentlyAssigned = isAssigned,
                Message = canAssign ? "Assignment is valid" : 
                         isAssigned ? "Assignment already exists" : "Assignment is not valid"
            };

            return Ok(validation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating assignment for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            return StatusCode(500, new { error = "An error occurred while validating the assignment" });
        }
    }
}