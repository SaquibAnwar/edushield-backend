namespace EduShield.Core.Dtos;

/// <summary>
/// Result of a payment operation
/// </summary>
public class PaymentResult
{
    /// <summary>
    /// Whether the payment was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Payment transaction ID
    /// </summary>
    public string? TransactionId { get; set; }
    
    /// <summary>
    /// Amount that was paid
    /// </summary>
    public decimal AmountPaid { get; set; }
    
    /// <summary>
    /// New amount due after payment
    /// </summary>
    public decimal NewAmountDue { get; set; }
    
    /// <summary>
    /// New payment status after payment
    /// </summary>
    public string NewPaymentStatus { get; set; } = string.Empty;
    
    /// <summary>
    /// Date and time of the payment
    /// </summary>
    public DateTime PaymentDate { get; set; }
    
    /// <summary>
    /// Error message if payment failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Updated fee record after payment
    /// </summary>
    public StudentFeeDto? UpdatedFee { get; set; }
}

