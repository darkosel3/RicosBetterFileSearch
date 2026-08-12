using RicosBetterFileSearch.Modules.Indexing.Domain.Entities;

namespace RicosBetterFileSearch.Modules.Indexing.Application.Ports;

public interface IFileSystemService
{
    Task<IEnumerable<FileEntry>> ScanFolderAsync(string folderPath, Guid folderId);
}
