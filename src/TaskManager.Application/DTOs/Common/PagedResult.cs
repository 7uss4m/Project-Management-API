namespace TaskManager.Application.DTOs.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public static PagedResult<T> From(IReadOnlyList<T> items, int total, int page, int pageSize) =>
        new() { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
}
