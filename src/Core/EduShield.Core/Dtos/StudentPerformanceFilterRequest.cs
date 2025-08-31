using EduShield.Core.Enums;

namespace EduShield.Core.Dtos;

/// <summary>
/// Filter request for student performance records
/// </summary>
public class StudentPerformanceFilterRequest : PaginationRequest
{
    /// <summary>
    /// Filter by student ID
    /// </summary>
    public Guid? StudentId { get; set; }

    /// <summary>
    /// Filter by subject
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Filter by exam type
    /// </summary>
    public ExamType? ExamType { get; set; }

    /// <summary>
    /// Filter by start date
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Filter by end date
    /// </summary>
    public DateTime? ToDate { get; set; }
}