namespace ECommerce.Application.DTOs.Common;

/// <summary>
/// Base class for paginated requests
/// </summary>
public class PagedRequest
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page (max 100)
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 10 : value);
    }

    /// <summary>
    /// Property name to sort by (optional)
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort in descending order
    /// </summary>
    public bool SortDescending { get; set; } = false;

    /// <summary>
    /// Search term for filtering (optional)
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Calculate skip count for database query
    /// </summary>
    public int Skip => (Page - 1) * PageSize;

    /// <summary>
    /// Filter by low stock items (<= 30)
    /// </summary>
    public bool LowStock { get; set; } = false;
}
