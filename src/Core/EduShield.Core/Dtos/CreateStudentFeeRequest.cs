using System.ComponentModel.DataAnnotations;
using EduShield.Core.Enums;

namespace EduShield.Core.Dtos;

/// <summary>
/// Request model for creating a new student fee record
/// </summary>
public class CreateStudentFeeRequest
{
    /// <summary>
    /// Student ID this fee belongs to
    /// </summary>
    [Required(ErrorMessage = "Student ID is required")]
    public Guid StudentId { get; set; }
    
    /// <summary>
    /// Type of fee
    /// </summary>
    [Required(ErrorMessage = "Fee type is required")]
    public FeeType FeeType { get; set; }
    
    /// <summary>
    /// Term for this fee (e.g., "2024-Q1", "2024-Q2")
    /// </summary>
    [Required(ErrorMessage = "Term is required")]
    [StringLength(20, ErrorMessage = "Term cannot exceed 20 characters")]
    public string Term { get; set; } = string.Empty;
    
    /// <summary>
    /// Total amount for this fee
    /// </summary>
    [Required(ErrorMessage = "Total amount is required")]
    [Range(0.01, 100000, ErrorMessage = "Total amount must be between 0.01 and 100,000")]
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Due date for this fee
    /// </summary>
    [Required(ErrorMessage = "Due date is required")]
    public DateTime DueDate { get; set; }
    
    /// <summary>
    /// Additional notes about this fee
    /// </summary>
    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}

