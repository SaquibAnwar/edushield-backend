using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduShield.Core.Entities;

/// <summary>
/// Represents the assignment relationship between a student and faculty member
/// </summary>
[Table("StudentFaculty")]
public class StudentFaculty : AuditableEntity
{
    /// <summary>
    /// ID of the faculty member
    /// </summary>
    [Required]
    public Guid FacultyId { get; set; }
    
    /// <summary>
    /// Navigation property to Faculty
    /// </summary>
    public Faculty Faculty { get; set; } = null!;
    
    /// <summary>
    /// ID of the student
    /// </summary>
    [Required]
    public Guid StudentId { get; set; }
    
    /// <summary>
    /// Navigation property to Student
    /// </summary>
    public Student Student { get; set; } = null!;
    
    /// <summary>
    /// Date when the assignment was made
    /// </summary>
    [Required]
    public DateTime AssignedDate { get; set; }
    
    /// <summary>
    /// Whether the assignment is currently active
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Subject for this assignment
    /// </summary>
    [MaxLength(100)]
    public string? Subject { get; set; }
    
    /// <summary>
    /// Academic year for this assignment
    /// </summary>
    [MaxLength(20)]
    public string? AcademicYear { get; set; }
    
    /// <summary>
    /// Semester for this assignment
    /// </summary>
    [MaxLength(20)]
    public string? Semester { get; set; }
    
    /// <summary>
    /// Optional notes about the assignment
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}