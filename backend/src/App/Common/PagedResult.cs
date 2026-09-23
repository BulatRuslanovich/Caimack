namespace App.Common;

public record PagedResult<T>(
    IReadOnlyList<T> Values,
    int Total,
    int Page,
    int PageSize
)
{
    public int TotalPage
    {
        get
        {
            if (PageSize <= 0)
            {
                return 0;
            } else
            {
                return (int)Math.Ceiling(Total / (double)PageSize);
            }
        }
    }

    public static PagedResult<T> Empty(PageRequest p) => new([], 0, p.Page, p.PageSize);
}

public record PageRequest
{
	private const int MaxPageSize = 200;

    public int Page { get; }
    public int PageSize { get; }

    public PageRequest(int? page = null, int? pageSize = null)
    {
        Page = page is null or < 1 ? 1 : page.Value;
        PageSize = pageSize is null or < 1 ? 50 : Math.Min(pageSize.Value, MaxPageSize);
    }

    public int Skip => (Page - 1) * PageSize;
}
