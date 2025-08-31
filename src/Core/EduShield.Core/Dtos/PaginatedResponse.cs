namespace EduShield.Core.Dtos;

/// <summary>
/// Paginated response wrapper
/// </summary>
/// <typeparam name="T">Type of data items</typeparam>
public class PaginatedResponse<T>
{
    /// <summary>
    /// Data items for current page
    /// </summary>
    public IEnumerable<T> Data { get; set; } = new List<T>();

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Default constructor
    /// </summary>
    public PaginatedResponse()
    {
    }

    /// <summary>
    /// Constructor for backward compatibility
    /// </summary>
    /// <param name="data">Data items</param>
    /// <param name="totalCount">Total count</param>
    /// <param name="page">Current page</param>
    /// <param name="pageSize">Page size</param>
    public PaginatedResponse(IEnumerable<T> data, int totalCount, int page, int pageSize)
    {
        Data = data;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }

    /// <summary>
    /// Create a paginated response
    /// </summary>
    /// <param name="data">Data items</param>
    /// <param name="totalCount">Total count</param>
    /// <param name="page">Current page</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated response</returns>
    public static PaginatedResponse<T> Create(IEnumerable<T> data, int totalCount, int page, int pageSize)
    {
        return new PaginatedResponse<T>
        {
            Data = data,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}