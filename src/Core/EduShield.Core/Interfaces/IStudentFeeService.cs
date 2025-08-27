using EduShield.Core.Dtos;
using EduShield.Core.Enums;

namespace EduShield.Core.Interfaces;

/// <summary>
/// Service interface for student fee business operations
/// </summary>
public interface IStudentFeeService
{
    /// <summary>
    /// Create a new fee record
    /// </summary>
    /// <param name="request">Fee creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created fee DTO</returns>
    Task<StudentFeeDto> CreateAsync(CreateStudentFeeRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a fee record by ID
    /// </summary>
    /// <param name="id">Fee record ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Fee DTO or null if not found</returns>
    Task<StudentFeeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all fee records for a specific student
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of fee DTOs for the student</returns>
    Task<IEnumerable<StudentFeeDto>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all fee records for a specific user (by user ID)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of fee DTOs for the user's student profile</returns>
    Task<IEnumerable<StudentFeeDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get fee records by fee type
    /// </summary>
    /// <param name="feeType">Type of fee</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of fee DTOs for the specified type</returns>
    Task<IEnumerable<StudentFeeDto>> GetByFeeTypeAsync(FeeType feeType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get fee records by term
    /// </summary>
    /// <param name="term">Term (e.g., "2024-Q1")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of fee DTOs for the specified term</returns>
    Task<IEnumerable<StudentFeeDto>> GetByTermAsync(string term, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get fee records by payment status
    /// </summary>
    /// <param name="status">Payment status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of fee DTOs with the specified status</returns>
    Task<IEnumerable<StudentFeeDto>> GetByPaymentStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get overdue fee records
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of overdue fee DTOs</returns>
    Task<IEnumerable<StudentFeeDto>> GetOverdueAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get fee records within a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of fee DTOs within the date range</returns>
    Task<IEnumerable<StudentFeeDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all fee records
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all fee DTOs</returns>
    Task<IEnumerable<StudentFeeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get fee records for students assigned to a specific faculty
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of fee DTOs for students assigned to the faculty</returns>
    Task<IEnumerable<StudentFeeDto>> GetByFacultyIdAsync(Guid facultyId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get fee records for children of a specific parent
    /// </summary>
    /// <param name="parentId">Parent ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of fee DTOs for the parent's children</returns>
    Task<IEnumerable<StudentFeeDto>> GetByParentIdAsync(Guid parentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update an existing fee record
    /// </summary>
    /// <param name="id">Fee record ID to update</param>
    /// <param name="request">Fee update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated fee DTO</returns>
    Task<StudentFeeDto> UpdateAsync(Guid id, UpdateStudentFeeRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete a fee record
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
    /// Make a payment on a fee record
    /// </summary>
    /// <param name="id">Fee record ID</param>
    /// <param name="request">Payment request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment result</returns>
    Task<PaymentResult> MakePaymentAsync(Guid id, PaymentRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get fee statistics for a student
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Fee statistics for the student</returns>
    Task<object> GetStudentFeeStatisticsAsync(Guid studentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Calculate and apply late fees for overdue records
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of records updated with late fees</returns>
    Task<int> CalculateLateFeesAsync(CancellationToken cancellationToken = default);
}
