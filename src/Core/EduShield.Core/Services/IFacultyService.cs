using EduShield.Core.Dtos;

namespace EduShield.Core.Services;

/// <summary>
/// Service interface for faculty business operations
/// </summary>
public interface IFacultyService
{
    /// <summary>
    /// Create a new faculty member
    /// </summary>
    /// <param name="request">Faculty creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created faculty DTO</returns>
    Task<FacultyDto> CreateAsync(CreateFacultyRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a faculty member by ID
    /// </summary>
    /// <param name="id">Faculty ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Faculty DTO or null if not found</returns>
    Task<FacultyDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a faculty member by email
    /// </summary>
    /// <param name="email">Faculty email</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Faculty DTO or null if not found</returns>
    Task<FacultyDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a faculty member by employee ID
    /// </summary>
    /// <param name="employeeId">Faculty employee ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Faculty DTO or null if not found</returns>
    Task<FacultyDto?> GetByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all faculty members
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all faculty DTOs</returns>
    Task<IEnumerable<FacultyDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get faculty members by department
    /// </summary>
    /// <param name="department">Department name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of faculty DTOs in the specified department</returns>
    Task<IEnumerable<FacultyDto>> GetByDepartmentAsync(string department, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get faculty members by subject
    /// </summary>
    /// <param name="subject">Subject name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of faculty DTOs teaching the specified subject</returns>
    Task<IEnumerable<FacultyDto>> GetBySubjectAsync(string subject, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active faculty members
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active faculty DTOs</returns>
    Task<IEnumerable<FacultyDto>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing faculty member
    /// </summary>
    /// <param name="id">Faculty ID to update</param>
    /// <param name="request">Faculty update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated faculty DTO</returns>
    Task<FacultyDto> UpdateAsync(Guid id, UpdateFacultyRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a faculty member
    /// </summary>
    /// <param name="id">Faculty ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a faculty member exists by ID
    /// </summary>
    /// <param name="id">Faculty ID to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if faculty exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if an email is already taken by another faculty member
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if email exists, false otherwise</returns>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if an employee ID is already taken by another faculty member
    /// </summary>
    /// <param name="employeeId">Employee ID to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if employee ID exists, false otherwise</returns>
    Task<bool> EmployeeIdExistsAsync(string employeeId, CancellationToken cancellationToken = default);
}
