using EduShield.Core.Enums;

namespace EduShield.Core.Dtos;

/// <summary>
/// Filter request for student fee records
/// </summary>
public class StudentFeeFilterRequest : PaginationRequest
{
    /// <summary>
    /// Filter by student ID
    /// </summary>
    public Guid? StudentId { get; set; }

    /// <summary>
    /// Filter by fee type
    /// </summary>
    public FeeType? FeeType { get; set; }

    /// <summary>
    /// Filter by payment status
    /// </summary>
    public PaymentStatus? PaymentStatus { get; set; }

    /// <summary>
    /// Filter by term
    /// </summary>
    public string? Term { get; set; }

    /// <summary>
    /// Filter by overdue status
    /// </summary>
    public bool? IsOverdue { get; set; }

    /// <summary>
    /// Filter by start date
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Filter by end date
    /// </summary>
    public DateTime? ToDate { get; set; }
}