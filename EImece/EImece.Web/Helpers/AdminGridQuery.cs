namespace EImece.Web.Helpers;

/// <summary>Parses grid query-string params shared by all Admin Index actions.</summary>
public sealed class AdminGridQuery
{
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;
    public string? Sort { get; init; }
    public string SortDir { get; init; } = "desc";

    public static AdminGridQuery From(string? search, int page = 1, int pageSize = 25, string? sort = null, string? sortDir = null)
    {
        page = Math.Max(1, page);
        pageSize = pageSize switch
        {
            <= 0 => 25,
            > 200 => 200,
            _ => pageSize
        };
        var dir = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";
        return new AdminGridQuery
        {
            Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            Page = page,
            PageSize = pageSize,
            Sort = string.IsNullOrWhiteSpace(sort) ? null : sort.Trim(),
            SortDir = dir
        };
    }

    public int Skip => (Page - 1) * PageSize;
}

public static class AdminGridLinq
{
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, AdminGridQuery grid)
        => query.Skip(grid.Skip).Take(grid.PageSize);
}
