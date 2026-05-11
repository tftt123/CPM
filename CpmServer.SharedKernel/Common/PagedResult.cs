namespace CpmServer.Common;

public class PagedResult<T>
{
    public List<T> List { get; set; } = new();
    public long Total { get; set; }
    public int PageNum { get; set; }
    public int PageSize { get; set; }

    public static PagedResult<T> Of(List<T> list, long total, int pageNum, int pageSize)
        => new() { List = list, Total = total, PageNum = pageNum, PageSize = pageSize };
}
