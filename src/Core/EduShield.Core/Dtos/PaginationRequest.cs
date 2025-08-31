namespace EduShield.Core.Dtos;

/// <summary>
/// Base pagination request parameters
/// </summary>
public class PaginationRequest
{
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int Limit { get; set; } = 10;

    /// <summary>
    /// Field to sort by
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort order (asc or desc)
    /// </summary>
    public string SortOrder { get; set; } = "asc";

    /// <summary>
    /// Search term for filtering
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Calculate skip count for database queries
    /// </summary>
    public int Skip => (Page - 1) * Limit;

    /// <summary>
    /// Validate pagination parameters
    /// </summary>
    public void Validate()
    {
        if (Page < 1) Page = 1;
        if (Limit < 1) Limit = 10;
        if (Limit > 100) Limit = 100; // Max limit to prevent abuse
        
        if (!string.IsNullOrEmpty(SortOrder) && 
            !SortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase) && 
            !SortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase))
        {
            SortOrder = "asc";
        }
    }
}