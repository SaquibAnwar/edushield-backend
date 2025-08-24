using EduShield.Core.Entities;

namespace EduShield.Core.Interfaces;

/// <summary>
/// Repository interface for faculty data operations
/// </summary>
public interface IFacultyRepository
{
    /// <summary>
    /// Get a faculty member by ID
    /// </summary>
    /// <param name="id">Faculty ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Faculty entity or null if not found</returns>
    Task<Faculty?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a faculty member by email
    /// </summary>
    /// <param name="email">Faculty email</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Faculty entity or null if not found</returns>
    Task<Faculty?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a faculty member by employee ID
    /// </summary>
    /// <param name="employeeId">Faculty employee ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Faculty entity or null if not found</returns>
    Task<Faculty?> GetByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all faculty members
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all faculty members</returns>
    Task<IEnumerable<Faculty>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get faculty members by department
    /// </summary>
    /// <param name="department">Department name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of faculty members in the specified department</returns>
    Task<IEnumerable<Faculty>> GetByDepartmentAsync(string department, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get faculty members by subject
    /// </summary>
    /// <param name="subject">Subject name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of faculty members teaching the specified subject</returns>
    Task<IEnumerable<Faculty>> GetBySubjectAsync(string subject, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active faculty members
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active faculty members</returns>
    Task<IEnumerable<Faculty>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new faculty member
    /// </summary>
    /// <param name="faculty">Faculty entity to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created faculty entity</returns>
    Task<Faculty> CreateAsync(Faculty faculty, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing faculty member
    /// </summary>
    /// <param name="faculty">Faculty entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated faculty entity</returns>
    Task<Faculty> UpdateAsync(Faculty faculty, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a faculty member by ID
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

    /// <summary>
    /// Generate the next available employee ID
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Next available employee ID</returns>
    Task<string> GenerateNextEmployeeIdAsync(CancellationToken cancellationToken = default);
}
