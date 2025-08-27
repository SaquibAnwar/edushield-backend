using EduShield.Core.Enums;

namespace EduShield.Core.Dtos;

/// <summary>
/// Data transfer object for student fee information
/// </summary>
public class StudentFeeDto
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Student this fee belongs to
    /// </summary>
    public Guid StudentId { get; set; }
    
    /// <summary>
    /// Student's first name
    /// </summary>
    public string StudentFirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// Student's last name
    /// </summary>
    public string StudentLastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Student's full name
    /// </summary>
    public string StudentFullName => $"{StudentFirstName} {StudentLastName}".Trim();
    
    /// <summary>
    /// Student's roll number
    /// </summary>
    public string StudentRollNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of fee
    /// </summary>
    public FeeType FeeType { get; set; }
    
    /// <summary>
    /// Term for this fee (e.g., "2024-Q1", "2024-Q2")
    /// </summary>
    public string Term { get; set; } = string.Empty;
    
    /// <summary>
    /// Total amount for this fee
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Amount already paid
    /// </summary>
    public decimal AmountPaid { get; set; }
    
    /// <summary>
    /// Amount still due
    /// </summary>
    public decimal AmountDue { get; set; }
    
    /// <summary>
    /// Current payment status
    /// </summary>
    public PaymentStatus PaymentStatus { get; set; }
    
    /// <summary>
    /// Due date for this fee
    /// </summary>
    public DateTime DueDate { get; set; }
    
    /// <summary>
    /// Date of last payment (if any)
    /// </summary>
    public DateTime? LastPaymentDate { get; set; }
    
    /// <summary>
    /// Late fee amount
    /// </summary>
    public decimal FineAmount { get; set; }
    
    /// <summary>
    /// Additional notes about this fee
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// When this fee record was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// When this fee record was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    // Computed properties
    /// <summary>
    /// Whether the fee is overdue
    /// </summary>
    public bool IsOverdue => DateTime.Today > DueDate && PaymentStatus != PaymentStatus.Paid;
    
    /// <summary>
    /// Days overdue (0 if not overdue)
    /// </summary>
    public int DaysOverdue => IsOverdue ? (DateTime.Today - DueDate).Days : 0;
    
    /// <summary>
    /// Formatted due date
    /// </summary>
    public string FormattedDueDate => DueDate.ToString("MMMM dd, yyyy");
    
    /// <summary>
    /// Formatted last payment date
    /// </summary>
    public string? FormattedLastPaymentDate => LastPaymentDate?.ToString("MMMM dd, yyyy");
    
    /// <summary>
    /// Formatted amounts for display
    /// </summary>
    public string FormattedTotalAmount => TotalAmount.ToString("C");
    public string FormattedAmountPaid => AmountPaid.ToString("C");
    public string FormattedAmountDue => AmountDue.ToString("C");
    public string FormattedFineAmount => FineAmount.ToString("C");
    
    /// <summary>
    /// Payment status description
    /// </summary>
    public string PaymentStatusDescription => PaymentStatus switch
    {
        PaymentStatus.Pending => "Pending",
        PaymentStatus.Partial => "Partial",
        PaymentStatus.Paid => "Paid",
        PaymentStatus.Overdue => "Overdue",
        _ => "Unknown"
    };
    
    /// <summary>
    /// Fee type description
    /// </summary>
    public string FeeTypeDescription => FeeType switch
    {
        FeeType.Tuition => "Tuition",
        FeeType.Exam => "Exam",
        FeeType.Transport => "Transport",
        FeeType.Library => "Library",
        FeeType.Misc => "Miscellaneous",
        _ => "Unknown"
    };
}
