using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EduShield.Core.Enums;

namespace EduShield.Core.Entities;

/// <summary>
/// Entity representing a student fee record
/// </summary>
[Table("StudentFees")]
public class StudentFee : AuditableEntity
{
    /// <summary>
    /// Unique identifier for the fee record
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Student this fee belongs to
    /// </summary>
    [Required]
    public Guid StudentId { get; set; }

    /// <summary>
    /// Navigation property to the student
    /// </summary>
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    /// <summary>
    /// Type of fee
    /// </summary>
    [Required]
    public FeeType FeeType { get; set; }

    /// <summary>
    /// Term for this fee (e.g., "2024-Q1", "2024-Q2")
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Term { get; set; } = string.Empty;

    /// <summary>
    /// Encrypted total amount for this fee
    /// </summary>
    [Required]
    public string EncryptedTotalAmount { get; set; } = string.Empty;

    /// <summary>
    /// Encrypted amount already paid
    /// </summary>
    [Required]
    public string EncryptedAmountPaid { get; set; } = string.Empty;

    /// <summary>
    /// Encrypted amount still due
    /// </summary>
    [Required]
    public string EncryptedAmountDue { get; set; } = string.Empty;

    /// <summary>
    /// Encrypted late fee amount
    /// </summary>
    [Required]
    public string EncryptedFineAmount { get; set; } = string.Empty;

    /// <summary>
    /// Current payment status
    /// </summary>
    [Required]
    public PaymentStatus PaymentStatus { get; set; }

    /// <summary>
    /// Due date for this fee
    /// </summary>
    [Required]
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Date of last payment (if any)
    /// </summary>
    public DateTime? LastPaymentDate { get; set; }

    /// <summary>
    /// Additional notes about this fee
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    // Computed properties for backward compatibility (these will be removed in future migrations)
    /// <summary>
    /// Total amount for this fee (computed property, will be removed)
    /// </summary>
    [NotMapped]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Amount already paid (computed property, will be removed)
    /// </summary>
    [NotMapped]
    public decimal AmountPaid { get; set; }

    /// <summary>
    /// Amount still due (computed property, will be removed)
    /// </summary>
    [NotMapped]
    public decimal AmountDue { get; set; }

    /// <summary>
    /// Late fee amount (computed property, will be removed)
    /// </summary>
    [NotMapped]
    public decimal FineAmount { get; set; }

    // Computed properties
    /// <summary>
    /// Whether the fee is overdue
    /// </summary>
    [NotMapped]
    public bool IsOverdue => DateTime.Today > DueDate && PaymentStatus != PaymentStatus.Paid;

    /// <summary>
    /// Days overdue (0 if not overdue)
    /// </summary>
    [NotMapped]
    public int DaysOverdue => IsOverdue ? (DateTime.Today - DueDate).Days : 0;

    /// <summary>
    /// Payment status description
    /// </summary>
    [NotMapped]
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
    [NotMapped]
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