using RicosBetterFileSearch.SharedKernel;

namespace RicosBetterFileSearch.Modules.Folders.Domain.Events;

public class FolderScannedEvent : IDomainEvent
{
    public Guid FolderId { get; }
    public string FolderPath { get; }
    public int FilesFound { get; }
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public FolderScannedEvent(Guid folderId, string folderPath, int filesFound)
    {
        FolderId = folderId;
        FolderPath = folderPath;
        FilesFound = filesFound;
    }
}
