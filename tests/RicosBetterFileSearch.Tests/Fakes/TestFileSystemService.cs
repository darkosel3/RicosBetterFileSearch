using RicosBetterFileSearch.Modules.Indexing.Application.Ports;
using RicosBetterFileSearch.Modules.Indexing.Domain.Entities;

namespace RicosBetterFileSearch.Tests.Fakes;

public class TestFileSystemService : IFileSystemService
{
    private readonly List<FileEntry> _files;

    public TestFileSystemService(List<FileEntry>? files = null)
    {
        _files = files ?? new List<FileEntry>
        {
            new() { FileName = "test.txt", FilePath = "/fake/test.txt", Extension = ".txt", SizeInBytes = 100 },
            new() { FileName = "photo.jpg", FilePath = "/fake/photo.jpg", Extension = ".jpg", SizeInBytes = 2048 },
            new() { FileName = "doc.pdf", FilePath = "/fake/doc.pdf", Extension = ".pdf", SizeInBytes = 5000 },
        };
    }

    public Task<IEnumerable<FileEntry>> ScanFolderAsync(string folderPath, Guid folderId)
    {
        foreach (var f in _files) f.FolderId = folderId;
        return Task.FromResult<IEnumerable<FileEntry>>(_files);
    }
}
