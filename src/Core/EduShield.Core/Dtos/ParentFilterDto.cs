using EduShield.Core.Enums;

namespace EduShield.Core.Dtos;

/// <summary>
/// DTO for filtering parents
/// </summary>
public class ParentFilterDto
{
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public ParentType? ParentType { get; set; }
    public bool? IsEmergencyContact { get; set; }
    public bool? IsAuthorizedToPickup { get; set; }
    public bool? IsActive { get; set; }
    public string? Occupation { get; set; }
    public DateTime? DateOfBirthFrom { get; set; }
    public DateTime? DateOfBirthTo { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
}


