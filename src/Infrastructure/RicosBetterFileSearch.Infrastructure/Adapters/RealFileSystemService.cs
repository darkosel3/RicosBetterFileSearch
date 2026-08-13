using RicosBetterFileSearch.Modules.Indexing.Application.Ports;
using RicosBetterFileSearch.Modules.Indexing.Domain.Entities;

namespace RicosBetterFileSearch.Infrastructure.Adapters;

/// <summary>
/// Fake adapter koji skenira pravi file system.
/// Za unit testove postoji InMemoryFileSystemService.
/// Heksagonalni pattern: Application sloj zavisi od IFileSystemService porta,
/// ne od konkretne implementacije.
/// </summary>
public class RealFileSystemService : IFileSystemService
{
    public Task<IEnumerable<FileEntry>> ScanFolderAsync(string folderPath, Guid folderId)
    {
        var entries = new List<FileEntry>();

        if (!Directory.Exists(folderPath))
            return Task.FromResult<IEnumerable<FileEntry>>(entries);

        try
        {
            var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);

            foreach (var filePath in files)
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
                catch
                {
                    // Skip fajlove koje ne mozemo da procitamo (permisije itd.)
                }
            }
        }
        catch
        {
            // Skip ako folder nije dostupan
        }

        return Task.FromResult<IEnumerable<FileEntry>>(entries);
    }
}
