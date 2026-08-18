using RicosBetterFileSearch.SharedKernel;
using RicosBetterFileSearch.Modules.Indexing.Domain.Entities;
using RicosBetterFileSearch.Modules.Indexing.Application.Ports;
using RicosBetterFileSearch.Modules.Folders.Domain.Entities;
using RicosBetterFileSearch.Modules.Folders.Domain.Events;

namespace RicosBetterFileSearch.Modules.Indexing.Application.UseCases;

public class IndexingUseCases
{
    private readonly IRepository<FileEntry> _fileRepository;
    private readonly IRepository<IndexedFolder> _folderRepository;
    private readonly IFileSystemService _fileSystemService;
    private readonly IEventBus _eventBus;

    public IndexingUseCases(
        IRepository<FileEntry> fileRepository,
        IRepository<IndexedFolder> folderRepository,
        IFileSystemService fileSystemService,
        IEventBus eventBus)
    {
        _fileRepository = fileRepository;
        _folderRepository = folderRepository;
        _fileSystemService = fileSystemService;
        _eventBus = eventBus;
    }

    public async Task<int> ScanFolderAsync(Guid folderId)
    {
        var folder = await _folderRepository.GetByIdAsync(folderId);
        if (folder is null) return 0;

        await _fileRepository.DeleteWhereAsync(f => f.FolderId == folderId);

        var scannedFiles = await _fileSystemService.ScanFolderAsync(folder.FolderPath, folderId);
        var fileList = scannedFiles.ToList();

        await _fileRepository.AddRangeAsync(fileList);

        folder.LastScannedAt = DateTime.UtcNow;
        folder.FileCount = fileList.Count;
        folder.UpdatedAt = DateTime.UtcNow;
        await _folderRepository.UpdateAsync(folder);

        _eventBus.Publish(new FolderScannedEvent(folderId, folder.FolderPath, fileList.Count));

        return fileList.Count;
    }

    public async Task<IEnumerable<FileEntry>> GetFilesByFolderAsync(Guid folderId)
    {
        var allFiles = await _fileRepository.GetAllAsync();
        return allFiles.Where(f => f.FolderId == folderId);
    }

    public async Task<IEnumerable<FileEntry>> GetAllFilesAsync()
    {
        return await _fileRepository.GetAllAsync();
    }
}
