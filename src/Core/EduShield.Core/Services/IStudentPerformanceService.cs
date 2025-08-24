using EduShield.Core.Dtos;
using EduShield.Core.Enums;

namespace EduShield.Core.Services;

/// <summary>
/// Service interface for student performance business operations
/// </summary>
public interface IStudentPerformanceService
{
    /// <summary>
    /// Create a new student performance record
    /// </summary>
    /// <param name="request">Performance creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created performance DTO</returns>
    Task<StudentPerformanceDto> CreateAsync(CreateStudentPerformanceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a performance record by ID
    /// </summary>
    /// <param name="id">Performance record ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Performance DTO or null if not found</returns>
    Task<StudentPerformanceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all performance records for a specific student
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of performance DTOs for the student</returns>
    Task<IEnumerable<StudentPerformanceDto>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance records by subject
    /// </summary>
    /// <param name="subject">Subject name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of performance DTOs for the subject</returns>
    Task<IEnumerable<StudentPerformanceDto>> GetBySubjectAsync(string subject, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance records by exam type
    /// </summary>
    /// <param name="examType">Type of exam</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of performance DTOs for the exam type</returns>
    Task<IEnumerable<StudentPerformanceDto>> GetByExamTypeAsync(ExamType examType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance records within a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of performance DTOs within the date range</returns>
    Task<IEnumerable<StudentPerformanceDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all performance records
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all performance DTOs</returns>
    Task<IEnumerable<StudentPerformanceDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance records for students assigned to a specific faculty
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of performance DTOs for students assigned to the faculty</returns>
    Task<IEnumerable<StudentPerformanceDto>> GetByFacultyIdAsync(Guid facultyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing performance record
    /// </summary>
    /// <param name="id">Performance record ID to update</param>
    /// <param name="request">Performance update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated performance DTO</returns>
    Task<StudentPerformanceDto> UpdateAsync(Guid id, UpdateStudentPerformanceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a performance record
    /// </summary>
    /// <param name="id">Performance record ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a performance record exists by ID
    /// </summary>
    /// <param name="id">Performance record ID to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if performance record exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance statistics for a student
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="subject">Optional subject filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Performance statistics for the student</returns>
    Task<object> GetStudentStatisticsAsync(Guid studentId, string? subject = null, CancellationToken cancellationToken = default);
}
