using RicosBetterFileSearch.Modules.Indexing.Application.Ports;
using RicosBetterFileSearch.Modules.Indexing.Domain.Entities;

namespace RicosBetterFileSearch.Infrastructure.Adapters;

public class RealFileSystemService : IFileSystemService 
{
    public Task<IEnumerable<FileEntry>> ScanFolderAsync(string folderPath, Guid folderId)
    {
        return Task.Run(() =>
        {
            var entries = new List<FileEntry>();

            if (!Directory.Exists(folderPath))
                return (IEnumerable<FileEntry>)entries;

            ScanRecursive(folderPath, folderId, entries);

            return (IEnumerable<FileEntry>)entries;
        });
    }

    private void ScanRecursive(string directory, Guid folderId, List<FileEntry> entries)
    {
        try
        {
            foreach (var filePath in Directory.GetFiles(directory))
            {
                try
                {
                    var info = new FileInfo(filePath);
                    entries.Add(new FileEntry
                    {
                        FileName = info.Name,
                        FilePath = info.FullName,
                        Extension = info.Extension.ToLowerInvariant(),
                        SizeInBytes = info.Length,
                        LastModified = info.LastWriteTimeUtc,
                        FolderId = folderId
                    });
                }
                catch { }
            }

            foreach (var subDir in Directory.GetDirectories(directory))
            {
                ScanRecursive(subDir, folderId, entries);
            }
        }
        catch { }
    }
}
