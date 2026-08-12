using RicosBetterFileSearch.SharedKernel;

namespace RicosBetterFileSearch.Modules.Indexing.Domain.Entities;

public class FileEntry : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public DateTime LastModified { get; set; }
    public Guid FolderId { get; set; }
}
