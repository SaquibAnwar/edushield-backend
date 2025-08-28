using EduShield.Core.Dtos;

namespace EduShield.Core.Interfaces;

/// <summary>
/// Service interface for managing faculty-student assignments
/// </summary>
public interface IFacultyStudentAssignmentService
{
    /// <summary>
    /// Assigns a student to a faculty member
    /// </summary>
    /// <param name="request">Assignment request</param>
    /// <returns>Assignment result</returns>
    Task<ServiceResult<FacultyStudentAssignmentDto>> AssignStudentToFacultyAsync(CreateFacultyStudentAssignmentRequest request);
    
    /// <summary>
    /// Assigns multiple students to a faculty member
    /// </summary>
    /// <param name="request">Bulk assignment request</param>
    /// <returns>Bulk assignment result</returns>
    Task<ServiceResult<List<FacultyStudentAssignmentDto>>> BulkAssignStudentsToFacultyAsync(BulkFacultyStudentAssignmentRequest request);
    
    /// <summary>
    /// Updates a faculty-student assignment
    /// </summary>
    /// <param name="request">Update request</param>
    /// <returns>Update result</returns>
    Task<ServiceResult<FacultyStudentAssignmentDto>> UpdateAssignmentAsync(UpdateFacultyStudentAssignmentRequest request);
    
    /// <summary>
    /// Deactivates a faculty-student assignment
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>Deactivation result</returns>
    Task<ServiceResult<bool>> DeactivateAssignmentAsync(Guid facultyId, Guid studentId);
    
    /// <summary>
    /// Activates a faculty-student assignment
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>Activation result</returns>
    Task<ServiceResult<bool>> ActivateAssignmentAsync(Guid facultyId, Guid studentId);
    
    /// <summary>
    /// Gets a faculty-student assignment
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>Assignment if found</returns>
    Task<ServiceResult<FacultyStudentAssignmentDto?>> GetAssignmentAsync(Guid facultyId, Guid studentId);
    
    /// <summary>
    /// Gets all assignments for a faculty member
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <returns>List of assignments</returns>
    Task<ServiceResult<List<FacultyStudentAssignmentDto>>> GetFacultyAssignmentsAsync(Guid facultyId);
    
    /// <summary>
    /// Gets all assignments for a student
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <returns>List of assignments</returns>
    Task<ServiceResult<List<FacultyStudentAssignmentDto>>> GetStudentAssignmentsAsync(Guid studentId);
    
    /// <summary>
    /// Gets faculty-student assignments with filtering and pagination
    /// </summary>
    /// <param name="filter">Filter criteria</param>
    /// <returns>Paginated list of assignments</returns>
    Task<ServiceResult<(List<FacultyStudentAssignmentDto> Assignments, int TotalCount)>> GetAssignmentsAsync(FacultyStudentAssignmentFilterDto filter);
    
    /// <summary>
    /// Gets faculty dashboard with assigned students
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <returns>Faculty dashboard data</returns>
    Task<ServiceResult<FacultyDashboardDto>> GetFacultyDashboardAsync(Guid facultyId);
    
    /// <summary>
    /// Checks if a student is assigned to a faculty member
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <param name="studentId">Student ID</param>
    /// <returns>True if assigned, false otherwise</returns>
    Task<ServiceResult<bool>> IsStudentAssignedToFacultyAsync(Guid facultyId, Guid studentId);
    
    /// <summary>
    /// Gets the count of active assignments for a faculty
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <returns>Count of active assignments</returns>
    Task<ServiceResult<int>> GetFacultyActiveAssignmentCountAsync(Guid facultyId);
}

/// <summary>
/// Generic service result wrapper
/// </summary>
/// <typeparam name="T">Type of the result data</typeparam>
public class ServiceResult<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = [];

    public static ServiceResult<T> CreateSuccess(T data, string? message = null)
    {
        return new ServiceResult<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ServiceResult<T> CreateFailure(string message, List<string>? errors = null)
    {
        return new ServiceResult<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? []
        };
    }
}

