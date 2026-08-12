using RicosBetterFileSearch.SharedKernel;

namespace RicosBetterFileSearch.Modules.Folders.Domain.Entities;

public class IndexedFolder : BaseEntity
{
    public string FolderPath { get; set; } = string.Empty;
    public string FolderName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? LastScannedAt { get; set; }
    public int FileCount { get; set; }
}
