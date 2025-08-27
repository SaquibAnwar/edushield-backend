namespace EduShield.Core.Enums;

/// <summary>
/// Status of fee payment
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment is pending
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Partial payment has been made
    /// </summary>
    Partial = 1,

    /// <summary>
    /// Full payment has been completed
    /// </summary>
    Paid = 2,

    /// <summary>
    /// Payment is overdue
    /// </summary>
    Overdue = 3
}
