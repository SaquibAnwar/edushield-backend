using EduShield.Core.Enums;

namespace EduShield.Core.Entities;

/// <summary>
/// Represents a student's academic performance record
/// </summary>
public class StudentPerformance : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    // Student relationship
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;
    
    // Academic information
    public string Subject { get; set; } = string.Empty;
    public ExamType ExamType { get; set; }
    public DateTime ExamDate { get; set; }
    
    // Encrypted score - stored as encrypted string in database
    public string EncryptedScore { get; set; } = string.Empty;
    
    // Additional metadata
    public string? Comments { get; set; }
    public decimal? MaxScore { get; set; } // Maximum possible score for this exam
    public string? ExamTitle { get; set; } // Optional exam title
    
    // Computed properties (not stored in database)
    public decimal Score { get; set; } // This will be set/retrieved via service layer
    public decimal? Percentage => MaxScore.HasValue && MaxScore.Value > 0 ? (Score / MaxScore.Value) * 100 : null;
    public string Grade => GetGrade();
    
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
