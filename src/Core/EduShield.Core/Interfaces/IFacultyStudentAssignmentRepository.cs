using EduShield.Core.Dtos;
using EduShield.Core.Entities;

namespace EduShield.Core.Interfaces;

/// <summary>
/// Repository interface for managing faculty-student assignments
/// </summary>
public interface IFacultyStudentAssignmentRepository
{
    /// <summary>
    /// Creates a new faculty-student assignment
    /// </summary>
    /// <param name="assignment">The assignment to create</param>
    /// <returns>The created assignment</returns>
    Task<StudentFaculty> CreateAsync(StudentFaculty assignment);
    
    /// <summary>
    /// Gets a faculty-student assignment by faculty and student IDs
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>The assignment if found, null otherwise</returns>
    Task<StudentFaculty?> GetByFacultyAndStudentAsync(Guid facultyId, Guid studentId);
    
    /// <summary>
    /// Gets all assignments for a specific faculty
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <returns>List of assignments for the faculty</returns>
    Task<List<StudentFaculty>> GetByFacultyIdAsync(Guid facultyId);
    
    /// <summary>
    /// Gets all assignments for a specific student
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <returns>List of assignments for the student</returns>
    Task<List<StudentFaculty>> GetByStudentIdAsync(Guid studentId);
    
    /// <summary>
    /// Gets all faculty-student assignments with optional filtering
    /// </summary>
    /// <param name="filter">Filter criteria</param>
    /// <returns>Paginated list of assignments</returns>
    Task<(List<StudentFaculty> Assignments, int TotalCount)> GetAssignmentsAsync(FacultyStudentAssignmentFilterDto filter);
    
    /// <summary>
    /// Updates an existing assignment
    /// </summary>
    /// <param name="assignment">The assignment to update</param>
    /// <returns>The updated assignment</returns>
    Task<StudentFaculty> UpdateAsync(StudentFaculty assignment);
    
    /// <summary>
    /// Deactivates a faculty-student assignment
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> DeactivateAsync(Guid facultyId, Guid studentId);
    
    /// <summary>
    /// Activates a faculty-student assignment
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> ActivateAsync(Guid facultyId, Guid studentId);
    
    /// <summary>
    /// Checks if a faculty-student assignment exists
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>True if assignment exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid facultyId, Guid studentId);
    
    /// <summary>
    /// Gets the count of active assignments for a faculty
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <returns>Count of active assignments</returns>
    Task<int> GetActiveAssignmentCountAsync(Guid facultyId);
    
    /// <summary>
    /// Gets the count of active assignments for a student
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <returns>Count of active assignments</returns>
    Task<int> GetStudentActiveAssignmentCountAsync(Guid studentId);
}

