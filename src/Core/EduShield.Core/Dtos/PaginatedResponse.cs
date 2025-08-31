namespace EduShield.Core.Dtos;

/// <summary>
/// Generic paginated response DTO
/// </summary>
/// <typeparam name="T">Type of data being paginated</typeparam>
public class PaginatedResponse<T>
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }

    public PaginatedResponse()
    {
    }

    public PaginatedResponse(IEnumerable<T> data, int totalCount, int page, int pageSize)
    {
        Data = data;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        HasNextPage = page < TotalPages;
        HasPreviousPage = page > 1;
    }
}