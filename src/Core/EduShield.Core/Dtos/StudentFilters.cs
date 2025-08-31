using EduShield.Core.Enums;

namespace EduShield.Core.Dtos;

/// <summary>
/// DTO for filtering students
/// </summary>
public class StudentFilters
{
    public string? Search { get; set; }
    public StudentStatus? Status { get; set; }
    public Gender? Gender { get; set; }
    public string? Grade { get; set; }
    public string? Section { get; set; }
    public Guid? ParentId { get; set; }
    public Guid? FacultyId { get; set; }
    public DateTime? EnrollmentDateFrom { get; set; }
    public DateTime? EnrollmentDateTo { get; set; }
    public DateTime? DateOfBirthFrom { get; set; }
    public DateTime? DateOfBirthTo { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public bool? IsEnrolled { get; set; }
    public int? Page { get; set; }
    public int? Limit { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}