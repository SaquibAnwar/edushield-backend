using EduShield.Core.Dtos;
using EduShield.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace EduShield.Core.Services;

/// <summary>
/// Mock payment service for development and testing
/// Simulates Stripe/Razorpay payment processing
/// </summary>
public class MockPaymentService : IPaymentService
{
    private readonly ILogger<MockPaymentService> _logger;
    private static readonly Random _random = new Random();
    
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
        _logger.LogInformation("Processing mock payment: {Amount} {Currency}", amount, currency);
        
        // Simulate payment processing delay
        await Task.Delay(100);
        
        // Simulate 95% success rate
        var isSuccess = _random.NextDouble() > 0.05;
        
        if (isSuccess)
        {
            var transactionId = GenerateTransactionId();
            
            _logger.LogInformation("Mock payment successful. Transaction ID: {TransactionId}", transactionId);
            
            return new PaymentResult
            {
                Success = true,
                TransactionId = transactionId,
                AmountPaid = amount,
                PaymentDate = DateTime.UtcNow
            };
        }
        else
        {
            _logger.LogWarning("Mock payment failed");
            
            return new PaymentResult
            {
                Success = false,
                ErrorMessage = "Payment failed due to insufficient funds or network error",
                PaymentDate = DateTime.UtcNow
            };
        }
    }
    
    public async Task<bool> VerifyPaymentAsync(string transactionId)
    {
        _logger.LogInformation("Verifying mock payment: {TransactionId}", transactionId);
        
        // Simulate verification delay
        await Task.Delay(50);
        
        // Mock verification - assume all transactions are valid
        return true;
    }
    
    public async Task<bool> RefundPaymentAsync(string transactionId, decimal? amount = null)
    {
        _logger.LogInformation("Processing mock refund: {TransactionId}, Amount: {Amount}", transactionId, amount);
        
        // Simulate refund processing delay
        await Task.Delay(200);
        
        // Mock refund - assume all refunds are successful
        return true;
    }
    
    private string GenerateTransactionId()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var random = _random.Next(1000, 9999);
        return $"mock_{timestamp}_{random}";
    }
}
