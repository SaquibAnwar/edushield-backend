using System.ComponentModel.DataAnnotations;
using EduShield.Core.Enums;

namespace EduShield.Core.Dtos;

/// <summary>
/// Request model for updating an existing student fee record
/// </summary>
public class UpdateStudentFeeRequest
{
    /// <summary>
    /// Type of fee
    /// </summary>
    public FeeType? FeeType { get; set; }
    
    /// <summary>
    /// Term for this fee (e.g., "2024-Q1", "2024-Q2")
    /// </summary>
    [StringLength(20, ErrorMessage = "Term cannot exceed 20 characters")]
    public string? Term { get; set; }
    
    /// <summary>
    /// Total amount for this fee
    /// </summary>
    [Range(0.01, 100000, ErrorMessage = "Total amount must be between 0.01 and 100,000")]
    public decimal? TotalAmount { get; set; }
    
    /// <summary>
    /// Amount already paid
    /// </summary>
    [Range(0, 100000, ErrorMessage = "Amount paid must be between 0 and 100,000")]
    public decimal? AmountPaid { get; set; }
    
    /// <summary>
    /// Due date for this fee
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// Additional notes about this fee
    /// </summary>
    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}
