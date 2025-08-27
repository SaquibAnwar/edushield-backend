using EduShield.Core.Enums;

namespace EduShield.Core.Entities;

/// <summary>
/// Represents a student fee record with payment tracking
/// </summary>
public class StudentFee : AuditableEntity
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Student this fee belongs to
    /// </summary>
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;
    
    /// <summary>
    /// Type of fee
    /// </summary>
    public FeeType FeeType { get; set; }
    
    /// <summary>
    /// Term for this fee (e.g., "2024-Q1", "2024-Q2")
    /// </summary>
    public string Term { get; set; } = string.Empty;
    
    /// <summary>
    /// Total amount for this fee (encrypted)
    /// </summary>
    public string EncryptedTotalAmount { get; set; } = string.Empty;
    
    /// <summary>
    /// Amount already paid (encrypted)
    /// </summary>
    public string EncryptedAmountPaid { get; set; } = string.Empty;
    
    /// <summary>
    /// Amount still due (encrypted)
    /// </summary>
    public string EncryptedAmountDue { get; set; } = string.Empty;
    
    /// <summary>
    /// Current payment status
    /// </summary>
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    
    /// <summary>
    /// Due date for this fee
    /// </summary>
    public DateTime DueDate { get; set; }
    
    /// <summary>
    /// Date of last payment (if any)
    /// </summary>
    public DateTime? LastPaymentDate { get; set; }
    
    /// <summary>
    /// Late fee amount (encrypted)
    /// </summary>
    public string EncryptedFineAmount { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional notes about this fee
    /// </summary>
    public string? Notes { get; set; }
    
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
}
