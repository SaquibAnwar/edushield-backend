using System.ComponentModel.DataAnnotations;

namespace EduShield.Core.Dtos;

/// <summary>
/// Request model for making a payment on a student fee record
/// </summary>
public class PaymentRequest
{
    /// <summary>
    /// Amount to pay
    /// </summary>
    [Required(ErrorMessage = "Payment amount is required")]
    [Range(0.01, 100000, ErrorMessage = "Payment amount must be between 0.01 and 100,000")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Payment method (e.g., "Credit Card", "Bank Transfer", "Cash")
    /// </summary>
    [Required(ErrorMessage = "Payment method is required")]
    [StringLength(50, ErrorMessage = "Payment method cannot exceed 50 characters")]
    public string PaymentMethod { get; set; } = string.Empty;
    
    /// <summary>
    /// Reference number for the payment
    /// </summary>
    [StringLength(100, ErrorMessage = "Reference number cannot exceed 100 characters")]
    public string? ReferenceNumber { get; set; }
    
    /// <summary>
    /// Additional notes about the payment
    /// </summary>
    [StringLength(200, ErrorMessage = "Payment notes cannot exceed 200 characters")]
    public string? Notes { get; set; }
}

