namespace EduShield.Core.Dtos;

/// <summary>
/// DTO for parent portal information
/// </summary>
public class ParentPortalDto
{
    public Guid ParentId { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public List<ParentPortalChildDto> Children { get; set; } = new();
    public DateTime LastLogin { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for child information in parent portal
/// </summary>
public class ParentPortalChildDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public List<ParentPortalPerformanceDto> Performance { get; set; } = new();
    public List<ParentPortalFeeDto> Fees { get; set; } = new();
}

/// <summary>
/// DTO for performance information in parent portal
/// </summary>
public class ParentPortalPerformanceDto
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public DateTime AssessmentDate { get; set; }
    public string AssessmentType { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
}

/// <summary>
/// DTO for fee information in parent portal
/// </summary>
public class ParentPortalFeeDto
{
    public Guid Id { get; set; }
    public string FeeType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? LastPaymentDate { get; set; }
}

