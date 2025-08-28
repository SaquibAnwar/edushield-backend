using System.ComponentModel.DataAnnotations;

namespace EduShield.Core.Dtos;

/// <summary>
/// DTO for creating parent-student assignments
/// </summary>
public class CreateParentStudentAssignmentDto
{
    [Required(ErrorMessage = "Parent ID is required")]
    public Guid ParentId { get; set; }
    
    [Required(ErrorMessage = "Student ID is required")]
    public Guid StudentId { get; set; }
    
    [Required(ErrorMessage = "Relationship is required")]
    [StringLength(50, ErrorMessage = "Relationship cannot exceed 50 characters")]
    public string Relationship { get; set; } = string.Empty;
    
    public bool IsPrimaryContact { get; set; } = false;
    public bool IsAuthorizedToPickup { get; set; } = true;
    public bool IsEmergencyContact { get; set; } = true;
    
    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating parent-student assignments
/// </summary>
public class UpdateParentStudentAssignmentDto
{
    [StringLength(50, ErrorMessage = "Relationship cannot exceed 50 characters")]
    public string? Relationship { get; set; }
    
    public bool? IsPrimaryContact { get; set; }
    public bool? IsAuthorizedToPickup { get; set; }
    public bool? IsEmergencyContact { get; set; }
    public bool? IsActive { get; set; }
    
    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for parent-student assignment responses
/// </summary>
public class ParentStudentAssignmentDto
{
    public Guid ParentId { get; set; }
    public string ParentFirstName { get; set; } = string.Empty;
    public string ParentLastName { get; set; } = string.Empty;
    public string ParentFullName { get; set; } = string.Empty;
    public string ParentEmail { get; set; } = string.Empty;
    public string ParentPhoneNumber { get; set; } = string.Empty;
    
    public Guid StudentId { get; set; }
    public string StudentFirstName { get; set; } = string.Empty;
    public string StudentLastName { get; set; } = string.Empty;
    public string StudentFullName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string StudentRollNumber { get; set; } = string.Empty;
    
    public string Relationship { get; set; } = string.Empty;
    public bool IsPrimaryContact { get; set; }
    public bool IsAuthorizedToPickup { get; set; }
    public bool IsEmergencyContact { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Computed properties
    public string RelationshipDescription => $"{ParentFullName} is {Relationship} of {StudentFullName}";
    public string ContactPermissions => $"Primary: {(IsPrimaryContact ? "Yes" : "No")}, Pickup: {(IsAuthorizedToPickup ? "Yes" : "No")}, Emergency: {(IsEmergencyContact ? "Yes" : "No")}";
}

/// <summary>
/// DTO for bulk parent-student assignment operations
/// </summary>
public class BulkParentStudentAssignmentDto
{
    [Required(ErrorMessage = "Parent ID is required")]
    public Guid ParentId { get; set; }
    
    [Required(ErrorMessage = "At least one student ID is required")]
    [MinLength(1, ErrorMessage = "At least one student ID is required")]
    public List<Guid> StudentIds { get; set; } = [];
    
    [Required(ErrorMessage = "Relationship is required")]
    [StringLength(50, ErrorMessage = "Relationship cannot exceed 50 characters")]
    public string Relationship { get; set; } = string.Empty;
    
    public bool IsPrimaryContact { get; set; } = false;
    public bool IsAuthorizedToPickup { get; set; } = true;
    public bool IsEmergencyContact { get; set; } = true;
    
    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for parent with their assigned students
/// </summary>
public class ParentWithStudentsDto
{
    public Guid ParentId { get; set; }
    public string ParentFirstName { get; set; } = string.Empty;
    public string ParentLastName { get; set; } = string.Empty;
    public string ParentFullName { get; set; } = string.Empty;
    public string ParentEmail { get; set; } = string.Empty;
    public string ParentPhoneNumber { get; set; } = string.Empty;
    
    public List<StudentAssignmentDto> AssignedStudents { get; set; } = [];
    public int TotalStudents => AssignedStudents.Count;
    public int ActiveStudents => AssignedStudents.Count(s => s.IsActive);
}

/// <summary>
/// DTO for student with their assigned parents
/// </summary>
public class StudentWithParentsDto
{
    public Guid StudentId { get; set; }
    public string StudentFirstName { get; set; } = string.Empty;
    public string StudentLastName { get; set; } = string.Empty;
    public string StudentFullName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string StudentRollNumber { get; set; } = string.Empty;
    
    public List<ParentAssignmentDto> AssignedParents { get; set; } = [];
    public int TotalParents => AssignedParents.Count;
    public int ActiveParents => AssignedParents.Count(p => p.IsActive);
    public ParentAssignmentDto? PrimaryParent => AssignedParents.FirstOrDefault(p => p.IsPrimaryContact && p.IsActive);
}

/// <summary>
/// DTO for student assignment details in parent context
/// </summary>
public class StudentAssignmentDto
{
    public Guid StudentId { get; set; }
    public string StudentFirstName { get; set; } = string.Empty;
    public string StudentLastName { get; set; } = string.Empty;
    public string StudentFullName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string StudentRollNumber { get; set; } = string.Empty;
    public string? StudentGrade { get; set; }
    public string? StudentSection { get; set; }
    
    public string Relationship { get; set; } = string.Empty;
    public bool IsPrimaryContact { get; set; }
    public bool IsAuthorizedToPickup { get; set; }
    public bool IsEmergencyContact { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    
    public DateTime AssignedDate { get; set; }
}

/// <summary>
/// DTO for parent assignment details in student context
/// </summary>
public class ParentAssignmentDto
{
    public Guid ParentId { get; set; }
    public string ParentFirstName { get; set; } = string.Empty;
    public string ParentLastName { get; set; } = string.Empty;
    public string ParentFullName { get; set; } = string.Empty;
    public string ParentEmail { get; set; } = string.Empty;
    public string ParentPhoneNumber { get; set; } = string.Empty;
    public string? ParentOccupation { get; set; }
    
    public string Relationship { get; set; } = string.Empty;
    public bool IsPrimaryContact { get; set; }
    public bool IsAuthorizedToPickup { get; set; }
    public bool IsEmergencyContact { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    
    public DateTime AssignedDate { get; set; }
}