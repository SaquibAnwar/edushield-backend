using EduShield.Core.Enums;

namespace EduShield.Core.Interfaces;

/// <summary>
/// Service interface for fee calculation operations
/// </summary>
public interface IFeeCalculatorService
{
    /// <summary>
    /// Calculate late fee based on due date
    /// </summary>
    /// <param name="dueDate">Due date of the fee</param>
    /// <param name="currentDate">Current date (defaults to today)</param>
    /// <returns>Late fee amount</returns>
    decimal CalculateLateFee(DateTime dueDate, DateTime? currentDate = null);

    /// <summary>
    /// Calculate amount due based on total amount, amount paid, and fine amount
    /// </summary>
    /// <param name="totalAmount">Total amount of the fee</param>
    /// <param name="amountPaid">Amount already paid</param>
    /// <param name="fineAmount">Fine amount</param>
    /// <returns>Amount still due</returns>
    decimal CalculateAmountDue(decimal totalAmount, decimal amountPaid, decimal fineAmount);

    /// <summary>
    /// Determine payment status based on amounts
    /// </summary>
    /// <param name="totalAmount">Total amount of the fee</param>
    /// <param name="amountPaid">Amount already paid</param>
    /// <param name="fineAmount">Fine amount</param>
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