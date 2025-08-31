using EduShield.Core.Dtos;
using EduShield.Core.Enums;

namespace EduShield.Core.Interfaces;

/// <summary>
/// Service interface for student fee business operations
/// </summary>
public interface IStudentFeeService
{
    /// <summary>
    /// Get all student fee records
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all fee records</returns>
    Task<IEnumerable<StudentFeeDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific fee record by ID
    /// </summary>
    /// <param name="id">Fee record ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Fee record if found, null otherwise</returns>
    Task<StudentFeeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get fee records by faculty ID (for students assigned to the faculty)
    /// </summary>
    /// <param name="facultyId">Faculty ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of fee records for students assigned to the faculty</returns>
    Task<IEnumerable<StudentFeeDto>> GetByFacultyIdAsync(Guid facultyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get fee records by student ID
    /// </summary>
    /// <param name="studentId">Student ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of fee records for the student</returns>
    Task<IEnumerable<StudentFeeDto>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get fee records by user ID (for students)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of fee records for the user</returns>
    Task<IEnumerable<StudentFeeDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get fee records by parent ID (for children of the parent)
    /// </summary>
    /// <param name="parentId">Parent ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of fee records for the parent's children</returns>
    Task<IEnumerable<StudentFeeDto>> GetByParentIdAsync(Guid parentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get fee records by fee type
    /// </summary>
    /// <param name="feeType">Fee type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of fee records for the specified type</returns>
    Task<IEnumerable<StudentFeeDto>> GetByFeeTypeAsync(FeeType feeType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get fee records by term
    /// </summary>
    /// <param name="term">Term (e.g., "2024-Q1")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of fee records for the specified term</returns>
    Task<IEnumerable<StudentFeeDto>> GetByTermAsync(string term, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get overdue fee records
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of overdue fee records</returns>
    Task<IEnumerable<StudentFeeDto>> GetOverdueAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new fee record
    /// </summary>
    /// <param name="request">Fee creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created fee record</returns>
    Task<StudentFeeDto> CreateAsync(CreateStudentFeeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing fee record
    /// </summary>
    /// <param name="id">Fee record ID</param>
    /// <param name="request">Fee update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated fee record</returns>
    Task<StudentFeeDto> UpdateAsync(Guid id, UpdateStudentFeeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a fee record
    /// </summary>
    /// <param name="id">Fee record ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a fee record exists
    /// </summary>
    /// <param name="id">Fee record ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists, false otherwise</returns>
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
    /// <returns>Fee statistics</returns>
    Task<object> GetStudentFeeStatisticsAsync(Guid studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate and apply late fees for overdue records
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of records updated</returns>
    Task<int> CalculateLateFeesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get paginated fee records with filtering
    /// </summary>
    /// <param name="filter">Filter and pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated fee records</returns>
    Task<PaginatedResponse<StudentFeeDto>> GetPaginatedAsync(StudentFeeFilterRequest filter, CancellationToken cancellationToken = default);
}