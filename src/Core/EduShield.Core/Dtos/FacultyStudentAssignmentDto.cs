using EduShield.Core.Enums;

namespace EduShield.Core.Dtos;

/// <summary>
/// DTO for creating a faculty-student assignment
/// </summary>
public class CreateFacultyStudentAssignmentRequest
{
    /// <summary>
    /// ID of the faculty member
    /// </summary>
    public Guid FacultyId { get; set; }
    
    /// <summary>
    /// ID of the student to assign
    /// </summary>
    public Guid StudentId { get; set; }
    
    /// <summary>
    /// Optional notes about the assignment
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating a faculty-student assignment
/// </summary>
public class UpdateFacultyStudentAssignmentRequest
{
    /// <summary>
    /// ID of the faculty member
    /// </summary>
    public Guid FacultyId { get; set; }
    
    /// <summary>
    /// ID of the student
    /// </summary>
    public Guid StudentId { get; set; }
    
    /// <summary>
    /// Whether the assignment is active
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Optional notes about the assignment
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for faculty-student assignment response
/// </summary>
public class FacultyStudentAssignmentDto
{
    /// <summary>
    /// ID of the faculty member
    /// </summary>
    public Guid FacultyId { get; set; }
    
    /// <summary>
    /// Faculty's full name
    /// </summary>
    public string FacultyName { get; set; } = string.Empty;
    
    /// <summary>
    /// Faculty's email
    /// </summary>
    public string FacultyEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Faculty's department
    /// </summary>
    public string FacultyDepartment { get; set; } = string.Empty;
    
    /// <summary>
    /// Faculty's subject
    /// </summary>
    public string FacultySubject { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the student
    /// </summary>
    public Guid StudentId { get; set; }
    
    /// <summary>
    /// Student's full name
    /// </summary>
    public string StudentName { get; set; } = string.Empty;
    
    /// <summary>
    /// Student's email
    /// </summary>
    public string StudentEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Student's roll number
    /// </summary>
    public string StudentRollNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Student's grade
    /// </summary>
    public string? StudentGrade { get; set; }
    
    /// <summary>
    /// Student's section
    /// </summary>
    public string? StudentSection { get; set; }
    
    /// <summary>
    /// Date when the assignment was created
    /// </summary>
    public DateTime AssignedDate { get; set; }
    
    /// <summary>
    /// Whether the assignment is currently active
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Optional notes about the assignment
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for faculty dashboard showing assigned students
/// </summary>
public class FacultyDashboardDto
{
    /// <summary>
    /// Faculty information
    /// </summary>
    public FacultyDto Faculty { get; set; } = new();
    
    /// <summary>
    /// Total number of assigned students
    /// </summary>
    public int TotalAssignedStudents { get; set; }
    
    /// <summary>
    /// Number of active assignments
    /// </summary>
    public int ActiveAssignments { get; set; }
    
    /// <summary>
    /// List of assigned students
    /// </summary>
    public List<AssignedStudentDto> AssignedStudents { get; set; } = [];
}

/// <summary>
/// DTO for assigned student information in faculty dashboard
/// </summary>
public class AssignedStudentDto
{
    /// <summary>
    /// Student ID
    /// </summary>
    public Guid StudentId { get; set; }
    
    /// <summary>
    /// Student's full name
    /// </summary>
    public string StudentName { get; set; } = string.Empty;
    
    /// <summary>
    /// Student's email
    /// </summary>
    public string StudentEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Student's roll number
    /// </summary>
    public string StudentRollNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Student's grade
    /// </summary>
    public string? StudentGrade { get; set; }
    
    /// <summary>
    /// Student's section
    /// </summary>
    public string? StudentSection { get; set; }
    
    /// <summary>
    /// Assignment date
    /// </summary>
    public DateTime AssignedDate { get; set; }
    
    /// <summary>
    /// Whether the assignment is active
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Optional notes about the assignment
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for bulk assignment operations
/// </summary>
public class BulkFacultyStudentAssignmentRequest
{
    /// <summary>
    /// ID of the faculty member
    /// </summary>
    public Guid FacultyId { get; set; }
    
    /// <summary>
    /// List of student IDs to assign
    /// </summary>
    public List<Guid> StudentIds { get; set; } = [];
    
    /// <summary>
    /// Optional notes for all assignments
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for assignment search and filtering
/// </summary>
public class FacultyStudentAssignmentFilterDto
{
    /// <summary>
    /// Filter by faculty ID
    /// </summary>
    public Guid? FacultyId { get; set; }
    
    /// <summary>
    /// Filter by student ID
    /// </summary>
    public Guid? StudentId { get; set; }
    
    /// <summary>
    /// Filter by assignment status
    /// </summary>
    public bool? IsActive { get; set; }
    
    /// <summary>
    /// Filter by department
    /// </summary>
    public string? Department { get; set; }
    
    /// <summary>
    /// Filter by grade
    /// </summary>
    public string? Grade { get; set; }
    
    /// <summary>
    /// Search by name (faculty or student)
    /// </summary>
    public string? SearchTerm { get; set; }
    
    /// <summary>
    /// Page number for pagination
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// Page size for pagination
    /// </summary>
    public int PageSize { get; set; } = 20;
}

