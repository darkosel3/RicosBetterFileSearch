using RicosBetterFileSearch.SharedKernel;

namespace RicosBetterFileSearch.Modules.Tags.Domain.Entities;

public class FileTag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#808080";
}
