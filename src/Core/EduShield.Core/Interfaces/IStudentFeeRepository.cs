using EduShield.Core.Entities;
using EduShield.Core.Enums;

namespace EduShield.Core.Interfaces;

/// <summary>
/// Repository interface for student fee data access
/// </summary>
public interface IStudentFeeRepository
{
    /// <summary>
    /// Get a fee record by ID
    /// </summary>
    /// <param name="id">Fee record ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Fee record or null if not found</returns>
    Task<StudentFee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all fee records for a specific student
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of fee records for the student</returns>
    Task<IEnumerable<StudentFee>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get fee records by fee type
    /// </summary>
    /// <param name="feeType">Type of fee</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of fee records for the specified type</returns>
    Task<IEnumerable<StudentFee>> GetByFeeTypeAsync(FeeType feeType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get fee records by term
    /// </summary>
    /// <param name="term">Term (e.g., "2024-Q1")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of fee records for the specified term</returns>
    Task<IEnumerable<StudentFee>> GetByTermAsync(string term, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get fee records by payment status
    /// </summary>
    /// <param name="status">Payment status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of fee records with the specified status</returns>
    Task<IEnumerable<StudentFee>> GetByPaymentStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get overdue fee records
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of overdue fee records</returns>
    Task<IEnumerable<StudentFee>> GetOverdueAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get fee records within a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of fee records within the date range</returns>
    Task<IEnumerable<StudentFee>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all fee records
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all fee records</returns>
    Task<IEnumerable<StudentFee>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get fee records for students assigned to a specific faculty
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of fee records for students assigned to the faculty</returns>
    Task<IEnumerable<StudentFee>> GetByFacultyIdAsync(Guid facultyId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get fee records for children of a specific parent
    /// </summary>
    /// <param name="parentId">Parent ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of fee records for the parent's children</returns>
    Task<IEnumerable<StudentFee>> GetByParentIdAsync(Guid parentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Create a new fee record
    /// </summary>
    /// <param name="fee">Fee record to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created fee record</returns>
    Task<StudentFee> CreateAsync(StudentFee fee, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update an existing fee record
    /// </summary>
    /// <param name="fee">Fee record to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated fee record</returns>
    Task<StudentFee> UpdateAsync(StudentFee fee, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete a fee record by ID
    /// </summary>
    /// <param name="id">Fee record ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if a fee record exists by ID
    /// </summary>
    /// <param name="id">Fee record ID to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if fee record exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get fee statistics for a student
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Fee statistics for the student</returns>
    Task<object> GetStudentFeeStatisticsAsync(Guid studentId, CancellationToken cancellationToken = default);
}

