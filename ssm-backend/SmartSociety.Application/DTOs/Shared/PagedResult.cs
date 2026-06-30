namespace SmartSociety.Application.DTOs;

/// <summary>
/// Generic wrapper for paginated results.
/// </summary>
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static PagedResult<T> Create(IEnumerable<T> allItems, int pageNumber, int pageSize, string? search, Func<T, string?>[]? searchFields = null)
    {
        var filtered = allItems;

        // Apply search filter if search term and searchable fields are provided
        if (!string.IsNullOrWhiteSpace(search) && searchFields != null)
        {
            var term = search.Trim();
            filtered = filtered.Where(item =>
                searchFields.Any(field =>
                    field(item)?.Contains(term, StringComparison.OrdinalIgnoreCase) == true));
        }

        var materialised = filtered.ToList();
        var totalCount = materialised.Count;
        var items = materialised
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
