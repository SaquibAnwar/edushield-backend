using System.ComponentModel.DataAnnotations;
using EduShield.Core.Enums;

namespace EduShield.Core.Dtos;

/// <summary>
/// Request model for creating a new student performance record
/// </summary>
public class CreateStudentPerformanceRequest
{
    /// <summary>
    /// Student ID for whom the performance record is being created
    /// </summary>
    /// <example>443abd4f-9e56-4adc-9eb7-7a0e2522dd2b</example>
    [Required(ErrorMessage = "Student ID is required")]
    public Guid StudentId { get; set; }
    
    /// <summary>
    /// Subject for the performance record
    /// </summary>
    /// <example>Mathematics</example>
    [Required(ErrorMessage = "Subject is required")]
    [StringLength(100, ErrorMessage = "Subject cannot exceed 100 characters")]
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of examination or assessment
    /// </summary>
    /// <example>MidTerm</example>
    [Required(ErrorMessage = "Exam type is required")]
    public ExamType ExamType { get; set; }
    
    /// <summary>
    /// Date when the exam was conducted
    /// </summary>
    /// <example>2025-01-15</example>
    [Required(ErrorMessage = "Exam date is required")]
    public DateTime ExamDate { get; set; }
    
    /// <summary>
    /// Score obtained by the student
    /// </summary>
    /// <example>85.5</example>
    [Required(ErrorMessage = "Score is required")]
    [Range(0, 1000, ErrorMessage = "Score must be between 0 and 1000")]
    public decimal Score { get; set; }
    
    /// <summary>
    /// Maximum possible score for this exam
    /// </summary>
    /// <example>100</example>
    [Range(0, 1000, ErrorMessage = "Maximum score must be between 0 and 1000")]
    public decimal? MaxScore { get; set; }
    
    /// <summary>
    /// Optional title for the exam
    /// </summary>
    /// <example>Mid-Term Mathematics Exam</example>
    [StringLength(200, ErrorMessage = "Exam title cannot exceed 200 characters")]
    public string? ExamTitle { get; set; }
    
    /// <summary>
    /// Additional comments about the performance
    /// </summary>
    /// <example>Good understanding of algebra concepts</example>
    [StringLength(500, ErrorMessage = "Comments cannot exceed 500 characters")]
    public string? Comments { get; set; }
}
