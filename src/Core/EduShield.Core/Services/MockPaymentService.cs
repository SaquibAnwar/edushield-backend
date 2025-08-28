using EduShield.Core.Dtos;
using EduShield.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace EduShield.Core.Services;

/// <summary>
/// Mock payment service implementation for testing and development
/// </summary>
public class MockPaymentService : IPaymentService
{
    private readonly ILogger<MockPaymentService> _logger;
    private readonly Dictionary<string, PaymentRecord> _paymentRecords = new();

    public MockPaymentService(ILogger<MockPaymentService> logger)
    {
        _logger = logger;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(
        decimal amount, 
        string currency = "INR", 
        string? description = null, 
        Dictionary<string, string>? metadata = null)
    {
        await Task.Delay(100); // Simulate network delay

        // Generate a mock transaction ID
        var transactionId = $"TXN_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8]}";

        // Simulate payment processing logic
        var isSuccessful = SimulatePaymentProcessing(amount);

        var paymentRecord = new PaymentRecord
        {
            TransactionId = transactionId,
            Amount = amount,
            Currency = currency,
            Description = description,
            Metadata = metadata ?? new Dictionary<string, string>(),
            ProcessedAt = DateTime.UtcNow,
            IsSuccessful = isSuccessful,
            IsVerified = isSuccessful,
            IsRefunded = false
        };

        _paymentRecords[transactionId] = paymentRecord;

        var result = new PaymentResult
        {
            Success = isSuccessful,
            TransactionId = transactionId,
            AmountPaid = isSuccessful ? amount : 0m,
            NewAmountDue = 0m, // This would be calculated by the calling service
            NewPaymentStatus = isSuccessful ? "Paid" : "Failed",
            PaymentDate = paymentRecord.ProcessedAt,
            ErrorMessage = isSuccessful ? null : "Payment processing failed (mock failure)"
        };

        if (isSuccessful)
        {
            _logger.LogInformation("Mock payment processed successfully: {TransactionId}, Amount: {Amount} {Currency}", 
                transactionId, amount, currency);
        }
        else
        {
            _logger.LogWarning("Mock payment failed: Amount: {Amount} {Currency}", amount, currency);
        }

        return result;
    }

    public async Task<bool> VerifyPaymentAsync(string transactionId)
    {
        await Task.Delay(50); // Simulate network delay

        if (_paymentRecords.TryGetValue(transactionId, out var record))
        {
            _logger.LogInformation("Mock payment verification: {TransactionId} - {Status}", 
                transactionId, record.IsVerified ? "Verified" : "Not Verified");
            return record.IsVerified;
        }

        _logger.LogWarning("Mock payment verification failed: Transaction not found - {TransactionId}", transactionId);
        return false;
    }

    public async Task<bool> RefundPaymentAsync(string transactionId, decimal? amount = null)
    {
        await Task.Delay(100); // Simulate network delay

        if (_paymentRecords.TryGetValue(transactionId, out var record))
        {
            if (!record.IsSuccessful || !record.IsVerified)
            {
                _logger.LogWarning("Mock refund failed: Payment not successful or verified - {TransactionId}", transactionId);
                return false;
            }

            if (record.IsRefunded)
            {
                _logger.LogWarning("Mock refund failed: Payment already refunded - {TransactionId}", transactionId);
                return false;
            }

            var refundAmount = amount ?? record.Amount;
            if (refundAmount > record.Amount)
            {
                _logger.LogWarning("Mock refund failed: Refund amount exceeds original amount - {TransactionId}", transactionId);
                return false;
            }

            // Simulate refund processing
            var isRefundSuccessful = SimulateRefundProcessing(refundAmount);

            if (isRefundSuccessful)
            {
                record.IsRefunded = true;
                record.RefundedAmount = refundAmount;
                record.RefundedAt = DateTime.UtcNow;

                _logger.LogInformation("Mock refund processed successfully: {TransactionId}, Amount: {Amount} {Currency}", 
                    transactionId, refundAmount, record.Currency);
            }
            else
            {
                _logger.LogWarning("Mock refund processing failed: {TransactionId}", transactionId);
            }

            return isRefundSuccessful;
        }

        _logger.LogWarning("Mock refund failed: Transaction not found - {TransactionId}", transactionId);
        return false;
    }

    /// <summary>
    /// Simulate payment processing with some randomness
    /// </summary>
    /// <param name="amount">Payment amount</param>
    /// <returns>True if payment should succeed, false otherwise</returns>
    private static bool SimulatePaymentProcessing(decimal amount)
    {
        // Simulate different failure scenarios
        if (amount <= 0)
            return false;

        if (amount > 100000) // Very large amounts might fail
            return Random.Shared.NextDouble() > 0.3; // 70% success rate

        if (amount < 10) // Very small amounts might fail
            return Random.Shared.NextDouble() > 0.1; // 90% success rate

        // Normal amounts have high success rate
        return Random.Shared.NextDouble() > 0.05; // 95% success rate
    }

    /// <summary>
    /// Simulate refund processing
    /// </summary>
    /// <param name="amount">Refund amount</param>
    /// <returns>True if refund should succeed, false otherwise</returns>
    private static bool SimulateRefundProcessing(decimal amount)
    {
        // Refunds generally have high success rate
        return Random.Shared.NextDouble() > 0.02; // 98% success rate
    }

    /// <summary>
    /// Internal record for tracking mock payments
    /// </summary>
    private class PaymentRecord
    {
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string? Description { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
        public DateTime ProcessedAt { get; set; }
        public bool IsSuccessful { get; set; }
        public bool IsVerified { get; set; }
        public bool IsRefunded { get; set; }
        public decimal? RefundedAmount { get; set; }
        public DateTime? RefundedAt { get; set; }
    }
}