using EduShield.Core.Enums;

namespace EduShield.Core.Dtos;

/// <summary>
/// Data transfer object for student performance information
/// </summary>
public class StudentPerformanceDto
{
    /// <summary>
    /// Unique identifier for the performance record
    /// </summary>
    /// <example>443abd4f-9e56-4adc-9eb7-7a0e2522dd2b</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Student ID for whom this performance record belongs
    /// </summary>
    /// <example>443abd4f-9e56-4adc-9eb7-7a0e2522dd2b</example>
    public Guid StudentId { get; set; }

    /// <summary>
    /// Student's first name
    /// </summary>
    /// <example>John</example>
    public string StudentFirstName { get; set; } = string.Empty;

    /// <summary>
    /// Student's last name
    /// </summary>
    /// <example>Doe</example>
    public string StudentLastName { get; set; } = string.Empty;

    /// <summary>
    /// Student's full name
    /// </summary>
    /// <example>John Doe</example>
    public string StudentFullName => $"{StudentFirstName} {StudentLastName}".Trim();

    /// <summary>
    /// Subject for the performance record
    /// </summary>
    /// <example>Mathematics</example>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Type of examination or assessment
    /// </summary>
    /// <example>MidTerm</example>
    public ExamType ExamType { get; set; }

    /// <summary>
    /// Date when the exam was conducted
    /// </summary>
    /// <example>2025-01-15T00:00:00Z</example>
    public DateTime ExamDate { get; set; }

    /// <summary>
    /// Score obtained by the student (decrypted)
    /// </summary>
    /// <example>85.5</example>
    public decimal Score { get; set; }

    /// <summary>
    /// Maximum possible score for this exam
    /// </summary>
    /// <example>100</example>
    public decimal? MaxScore { get; set; }

    /// <summary>
    /// Optional title for the exam
    /// </summary>
    /// <example>Mid-Term Mathematics Exam</example>
    public string? ExamTitle { get; set; }

    /// <summary>
    /// Additional comments about the performance
    /// </summary>
    /// <example>Good understanding of algebra concepts</example>
    public string? Comments { get; set; }

    /// <summary>
    /// Date and time when the performance record was created
    /// </summary>
    /// <example>2025-01-15T10:30:00Z</example>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the performance record was last updated
    /// </summary>
    /// <example>2025-01-15T10:30:00Z</example>
    public DateTime UpdatedAt { get; set; }

    // Computed properties
    /// <summary>
    /// Percentage score (computed from Score and MaxScore)
    /// </summary>
    /// <example>85.5</example>
    public decimal? Percentage => MaxScore.HasValue && MaxScore.Value > 0 ? (Score / MaxScore.Value) * 100 : null;

    /// <summary>
    /// Letter grade based on percentage (computed)
    /// </summary>
    /// <example>A</example>
    public string Grade => GetGrade();

    /// <summary>
    /// Formatted exam date for display
    /// </summary>
    /// <example>January 15, 2025</example>
    public string FormattedExamDate => ExamDate.ToString("MMMM dd, yyyy");

    /// <summary>
    /// Formatted score with max score if available
    /// </summary>
    /// <example>85.5/100</example>
    public string FormattedScore => MaxScore.HasValue ? $"{Score:F1}/{MaxScore:F1}" : Score.ToString("F1");

    private string GetGrade()
    {
        if (!Percentage.HasValue) return "N/A";
        
        return Percentage.Value switch
        {
            >= 90 => "A+",
            >= 85 => "A",
            >= 80 => "A-",
            >= 75 => "B+",
            >= 70 => "B",
            >= 65 => "B-",
            >= 60 => "C+",
            >= 55 => "C",
            >= 50 => "C-",
            >= 45 => "D+",
            >= 40 => "D",
            >= 35 => "D-",
            _ => "F"
        };
    }
}
