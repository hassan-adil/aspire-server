namespace Shared.Kernel.Models;

public class PagedRequest
{
    public PagedRequest(bool normalize = true)
    {
        if (normalize)
            Normalize();
    }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
    public string? Filter { get; set; }
    public bool FilterStrict { get; set; } = false;
    public List<string>? FilterOn { get; set; }
    public int Skip => (PageNumber - 1) * PageSize;

    public void Normalize()
    {
        FilterOn = FilterOn?
            .Select(x => x.ToLowerInvariant())
            .ToList();
    }
}
