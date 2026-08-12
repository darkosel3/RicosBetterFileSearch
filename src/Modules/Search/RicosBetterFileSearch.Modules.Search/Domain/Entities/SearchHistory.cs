using RicosBetterFileSearch.SharedKernel;

namespace RicosBetterFileSearch.Modules.Search.Domain.Entities;

public class SearchHistory : BaseEntity
{
    public string Query { get; set; } = string.Empty;
    public int ResultCount { get; set; }
    public string? ExtensionFilter { get; set; }
}
