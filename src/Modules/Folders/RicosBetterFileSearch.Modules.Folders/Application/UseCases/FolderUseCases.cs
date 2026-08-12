using RicosBetterFileSearch.SharedKernel;
using RicosBetterFileSearch.Modules.Folders.Domain.Entities;

namespace RicosBetterFileSearch.Modules.Folders.Application.UseCases;

public class FolderUseCases
{
    private readonly IRepository<IndexedFolder> _folderRepository;

    public FolderUseCases(IRepository<IndexedFolder> folderRepository)
    {
        _folderRepository = folderRepository;
    }

    public async Task<IndexedFolder> AddFolderAsync(string folderPath)
    {
        var folder = new IndexedFolder
        {
            FolderPath = folderPath,
            FolderName = Path.GetFileName(folderPath) ?? folderPath
        };
        await _folderRepository.AddAsync(folder);
        return folder;
    }

    public async Task<IEnumerable<IndexedFolder>> GetAllFoldersAsync()
    {
        return await _folderRepository.GetAllAsync();
    }

    public async Task RemoveFolderAsync(Guid folderId)
    {
        await _folderRepository.DeleteAsync(folderId);
    }

    public async Task ToggleFolderAsync(Guid folderId)
    {
        var folder = await _folderRepository.GetByIdAsync(folderId);
        if (folder is not null)
        {
            folder.IsActive = !folder.IsActive;
            folder.UpdatedAt = DateTime.UtcNow;
            await _folderRepository.UpdateAsync(folder);
        }
    }
}
