using RicosBetterFileSearch.Modules.Indexing.Application.Ports;
using RicosBetterFileSearch.Modules.Indexing.Domain.Entities;

namespace RicosBetterFileSearch.Infrastructure.Adapters;

public class FakeFileSystemService : IFileSystemService
{
    public Task<IEnumerable<FileEntry>> ScanFolderAsync(string folderPath, Guid folderId)
    {
        var fakeFiles = new List<FileEntry>
        {
            new() { FileName = "document.pdf", FilePath = $"{folderPath}/document.pdf", Extension = ".pdf", SizeInBytes = 1024000, LastModified = DateTime.UtcNow.AddDays(-5), FolderId = folderId },
            new() { FileName = "photo.jpg", FilePath = $"{folderPath}/photo.jpg", Extension = ".jpg", SizeInBytes = 2048000, LastModified = DateTime.UtcNow.AddDays(-3), FolderId = folderId },
            new() { FileName = "notes.txt", FilePath = $"{folderPath}/notes.txt", Extension = ".txt", SizeInBytes = 512, LastModified = DateTime.UtcNow.AddDays(-1), FolderId = folderId },
            new() { FileName = "spreadsheet.xlsx", FilePath = $"{folderPath}/spreadsheet.xlsx", Extension = ".xlsx", SizeInBytes = 350000, LastModified = DateTime.UtcNow.AddDays(-10), FolderId = folderId },
            new() { FileName = "presentation.pptx", FilePath = $"{folderPath}/presentation.pptx", Extension = ".pptx", SizeInBytes = 5200000, LastModified = DateTime.UtcNow.AddDays(-2), FolderId = folderId },
            new() { FileName = "readme.txt", FilePath = $"{folderPath}/readme.txt", Extension = ".txt", SizeInBytes = 256, LastModified = DateTime.UtcNow, FolderId = folderId },
            new() { FileName = "backup.zip", FilePath = $"{folderPath}/backup.zip", Extension = ".zip", SizeInBytes = 10485760, LastModified = DateTime.UtcNow.AddDays(-30), FolderId = folderId },
        };

        return Task.FromResult<IEnumerable<FileEntry>>(fakeFiles);
    }
}
