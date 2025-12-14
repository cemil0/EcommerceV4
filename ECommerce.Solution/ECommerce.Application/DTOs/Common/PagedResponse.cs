namespace ECommerce.Application.DTOs.Common;

/// <summary>
/// Generic paginated response wrapper
/// </summary>
/// <typeparam name="T">Type of items in the response</typeparam>
public class PagedResponse<T>
{
    /// <summary>
    /// List of items for current page
    /// </summary>
    public List<T> Data { get; set; } = new();

    /// <summary>
    /// Current page number
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPrevious => Page > 1;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNext => Page < TotalPages;

    /// <summary>
    /// Create a new paged response
    /// </summary>
    public PagedResponse()
    {
    }

    /// <summary>
    /// Create a new paged response with data
    /// </summary>
    public PagedResponse(List<T> data, int page, int pageSize, int totalCount)
    {
        Data = data;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}
