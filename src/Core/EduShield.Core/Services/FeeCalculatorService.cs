using EduShield.Core.Enums;

namespace EduShield.Core.Services;

/// <summary>
/// Service for calculating fees, late fees, and payment amounts
/// </summary>
public interface IFeeCalculatorService
{
    /// <summary>
    /// Calculate late fee amount based on due date
    /// </summary>
    /// <param name="dueDate">Original due date</param>
    /// <param name="currentDate">Current date (defaults to today)</param>
    /// <returns>Late fee amount</returns>
    decimal CalculateLateFee(DateTime dueDate, DateTime? currentDate = null);
    
    /// <summary>
    /// Calculate amount due after payment
    /// </summary>
    /// <param name="totalAmount">Total fee amount</param>
    /// <param name="amountPaid">Amount already paid</param>
    /// <param name="fineAmount">Late fee amount</param>
    /// <returns>Amount still due</returns>
    decimal CalculateAmountDue(decimal totalAmount, decimal amountPaid, decimal fineAmount);
    
    /// <summary>
    /// Determine payment status based on amounts
    /// </summary>
    /// <param name="totalAmount">Total fee amount</param>
    /// <param name="amountPaid">Amount already paid</param>
    /// <param name="fineAmount">Late fee amount</param>
    /// <returns>Payment status</returns>
    PaymentStatus DeterminePaymentStatus(decimal totalAmount, decimal amountPaid, decimal fineAmount);
    
    /// <summary>
    /// Check if a fee is overdue
    /// </summary>
    /// <param name="dueDate">Due date</param>
    /// <param name="currentDate">Current date (defaults to today)</param>
    /// <returns>True if overdue, false otherwise</returns>
    bool IsOverdue(DateTime dueDate, DateTime? currentDate = null);
    
    /// <summary>
    /// Calculate days overdue
    /// </summary>
    /// <param name="dueDate">Due date</param>
    /// <param name="currentDate">Current date (defaults to today)</param>
    /// <returns>Number of days overdue (0 if not overdue)</returns>
    int CalculateDaysOverdue(DateTime dueDate, DateTime? currentDate = null);
}

/// <summary>
/// Implementation of fee calculation service
/// </summary>
public class FeeCalculatorService : IFeeCalculatorService
{
    private const decimal BaseLateFee = 100m; // ₹100 base late fee
    private const decimal DailyLateFee = 10m; // ₹10 per day after due
    private const decimal MaxLateFee = 500m;  // ₹500 maximum late fee
    
    public decimal CalculateLateFee(DateTime dueDate, DateTime? currentDate = null)
    {
        var now = currentDate ?? DateTime.Today;
        
        if (now <= dueDate)
            return 0m;
        
        var daysOverdue = (now - dueDate).Days;
        var lateFee = BaseLateFee + (daysOverdue * DailyLateFee);
        
        return Math.Min(lateFee, MaxLateFee);
    }
    
    public decimal CalculateAmountDue(decimal totalAmount, decimal amountPaid, decimal fineAmount)
    {
        var totalOwed = totalAmount + fineAmount;
        var amountDue = totalOwed - amountPaid;
        
        return Math.Max(amountDue, 0m); // Cannot have negative amount due
    }
    
    public PaymentStatus DeterminePaymentStatus(decimal totalAmount, decimal amountPaid, decimal fineAmount)
    {
        var totalOwed = totalAmount + fineAmount;
        
        if (amountPaid >= totalOwed)
            return PaymentStatus.Paid;
        
        if (amountPaid > 0)
            return PaymentStatus.Partial;
        
        // Check if overdue
        if (fineAmount > 0)
            return PaymentStatus.Overdue;
        
        return PaymentStatus.Pending;
    }
    
    public bool IsOverdue(DateTime dueDate, DateTime? currentDate = null)
    {
        var now = currentDate ?? DateTime.Today;
        return now > dueDate;
    }
    
    public int CalculateDaysOverdue(DateTime dueDate, DateTime? currentDate = null)
    {
        var now = currentDate ?? DateTime.Today;
        
        if (now <= dueDate)
            return 0;
        
        return (now - dueDate).Days;
    }
}
