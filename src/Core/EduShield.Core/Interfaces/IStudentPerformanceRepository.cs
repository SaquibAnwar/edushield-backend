using EduShield.Core.Entities;
using EduShield.Core.Enums;

namespace EduShield.Core.Interfaces;

/// <summary>
/// Repository interface for student performance data operations
/// </summary>
public interface IStudentPerformanceRepository
{
    /// <summary>
    /// Get a performance record by ID
    /// </summary>
    /// <param name="id">Performance record ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Performance record or null if not found</returns>
    Task<StudentPerformance?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all performance records for a specific student
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of performance records for the student</returns>
    Task<IEnumerable<StudentPerformance>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance records by subject
    /// </summary>
    /// <param name="subject">Subject name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of performance records for the subject</returns>
    Task<IEnumerable<StudentPerformance>> GetBySubjectAsync(string subject, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance records by exam type
    /// </summary>
    /// <param name="examType">Type of exam</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of performance records for the exam type</returns>
    Task<IEnumerable<StudentPerformance>> GetByExamTypeAsync(ExamType examType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance records within a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of performance records within the date range</returns>
    Task<IEnumerable<StudentPerformance>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all performance records
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all performance records</returns>
    Task<IEnumerable<StudentPerformance>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance records for students assigned to a specific faculty
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of performance records for students assigned to the faculty</returns>
    Task<IEnumerable<StudentPerformance>> GetByFacultyIdAsync(Guid facultyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new performance record
    /// </summary>
    /// <param name="performance">Performance record to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created performance record</returns>
    Task<StudentPerformance> CreateAsync(StudentPerformance performance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing performance record
    /// </summary>
    /// <param name="performance">Performance record to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated performance record</returns>
    Task<StudentPerformance> UpdateAsync(StudentPerformance performance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a performance record by ID
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
