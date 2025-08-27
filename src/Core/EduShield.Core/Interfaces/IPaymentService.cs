using EduShield.Core.Dtos;

namespace EduShield.Core.Interfaces;

/// <summary>
/// Service interface for payment processing
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Process a payment
    /// </summary>
    /// <param name="amount">Amount to pay</param>
    /// <param name="currency">Currency code (default: INR)</param>
    /// <param name="description">Payment description</param>
    /// <param name="metadata">Additional payment metadata</param>
    /// <returns>Payment result</returns>
    Task<PaymentResult> ProcessPaymentAsync(
        decimal amount, 
        string currency = "INR", 
        string? description = null, 
        Dictionary<string, string>? metadata = null);
    
    /// <summary>
    /// Verify a payment
    /// </summary>
    /// <param name="transactionId">Transaction ID to verify</param>
    /// <returns>True if payment is verified, false otherwise</returns>
    Task<bool> VerifyPaymentAsync(string transactionId);
    
    /// <summary>
    /// Refund a payment
    /// </summary>
    /// <param name="transactionId">Transaction ID to refund</param>
    /// <param name="amount">Amount to refund (null for full refund)</param>
    /// <returns>True if refund is successful, false otherwise</returns>
    Task<bool> RefundPaymentAsync(string transactionId, decimal? amount = null);
}
