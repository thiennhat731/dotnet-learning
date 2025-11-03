public class PaginationResponse<T>
{
    public MetaData meta { get; set; }
    public IEnumerable<T> result { get; set; } = Enumerable.Empty<T>();

    public PaginationResponse(IEnumerable<T> items, int page, int pageSize, long total)
    {
        result = items;
        meta = new MetaData
        {
            page = page,
            pageSize = pageSize,
            total = total,
            pages = (int)Math.Ceiling(total / (double)pageSize)
        };
    }
}

public class MetaData
{
    public int page { get; set; }
    public int pageSize { get; set; }
    public int pages { get; set; }
    public long total { get; set; }
}
