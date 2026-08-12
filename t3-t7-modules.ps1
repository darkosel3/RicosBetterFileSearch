# T3-T7: Add module references and copy files
# Pokreni iz C:\Users\Darko\source\repos\RicosBetterFileSearch

# ============================================
# PROJECT REFERENCES (medjumodulske zavisnosti)
# ============================================
dotnet add src/Modules/Indexing/RicosBetterFileSearch.Modules.Indexing reference src/Modules/Folders/RicosBetterFileSearch.Modules.Folders
dotnet add src/Modules/Search/RicosBetterFileSearch.Modules.Search reference src/Modules/Indexing/RicosBetterFileSearch.Modules.Indexing
dotnet add src/Modules/Statistics/RicosBetterFileSearch.Modules.Statistics reference src/Modules/Indexing/RicosBetterFileSearch.Modules.Indexing
dotnet add src/Modules/Statistics/RicosBetterFileSearch.Modules.Statistics reference src/Modules/Folders/RicosBetterFileSearch.Modules.Folders

# ============================================
# FOLDERS MODUL
# ============================================
$foldersBase = "src/Modules/Folders/RicosBetterFileSearch.Modules.Folders"

@'
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
'@ | Set-Content "$foldersBase/Domain/Entities/IndexedFolder.cs"

@'
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
'@ | Set-Content "$foldersBase/Domain/Events/FolderScannedEvent.cs"

@'
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
'@ | Set-Content "$foldersBase/Application/UseCases/FolderUseCases.cs"

# ============================================
# INDEXING MODUL
# ============================================
$indexingBase = "src/Modules/Indexing/RicosBetterFileSearch.Modules.Indexing"

@'
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
'@ | Set-Content "$indexingBase/Domain/Entities/FileEntry.cs"

@'
using RicosBetterFileSearch.Modules.Indexing.Domain.Entities;

namespace RicosBetterFileSearch.Modules.Indexing.Application.Ports;

public interface IFileSystemService
{
    Task<IEnumerable<FileEntry>> ScanFolderAsync(string folderPath, Guid folderId);
}
'@ | Set-Content "$indexingBase/Application/Ports/IFileSystemService.cs"

@'
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

        // Obrisi stare zapise za ovaj folder
        var existingFiles = (await _fileRepository.GetAllAsync())
            .Where(f => f.FolderId == folderId).ToList();
        foreach (var file in existingFiles)
        {
            await _fileRepository.DeleteAsync(file.Id);
        }

        // Skeniraj kroz IFileSystemService port (moze biti pravi FS ili fake)
        var scannedFiles = await _fileSystemService.ScanFolderAsync(folder.FolderPath, folderId);
        var fileList = scannedFiles.ToList();

        foreach (var file in fileList)
        {
            await _fileRepository.AddAsync(file);
        }

        // Azuriraj folder metadata
        folder.LastScannedAt = DateTime.UtcNow;
        folder.FileCount = fileList.Count;
        folder.UpdatedAt = DateTime.UtcNow;
        await _folderRepository.UpdateAsync(folder);

        // Publish event - Statistics modul reaguje
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
'@ | Set-Content "$indexingBase/Application/UseCases/IndexingUseCases.cs"

# ============================================
# TAGS MODUL
# ============================================
$tagsBase = "src/Modules/Tags/RicosBetterFileSearch.Modules.Tags"

@'
using RicosBetterFileSearch.SharedKernel;

namespace RicosBetterFileSearch.Modules.Tags.Domain.Entities;

public class FileTag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#808080";
}
'@ | Set-Content "$tagsBase/Domain/Entities/FileTag.cs"

@'
using RicosBetterFileSearch.SharedKernel;

namespace RicosBetterFileSearch.Modules.Tags.Domain.Entities;

public class FileTagAssignment : BaseEntity
{
    public Guid FileId { get; set; }
    public Guid TagId { get; set; }
}
'@ | Set-Content "$tagsBase/Domain/Entities/FileTagAssignment.cs"

@'
using RicosBetterFileSearch.SharedKernel;

namespace RicosBetterFileSearch.Modules.Tags.Domain.Events;

public class FileTaggedEvent : IDomainEvent
{
    public Guid FileId { get; }
    public Guid TagId { get; }
    public string TagName { get; }
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public FileTaggedEvent(Guid fileId, Guid tagId, string tagName)
    {
        FileId = fileId;
        TagId = tagId;
        TagName = tagName;
    }
}
'@ | Set-Content "$tagsBase/Domain/Events/FileTaggedEvent.cs"

@'
using RicosBetterFileSearch.SharedKernel;
using RicosBetterFileSearch.Modules.Tags.Domain.Entities;
using RicosBetterFileSearch.Modules.Tags.Domain.Events;

namespace RicosBetterFileSearch.Modules.Tags.Application.UseCases;

public class TagUseCases
{
    private readonly IRepository<FileTag> _tagRepository;
    private readonly IRepository<FileTagAssignment> _assignmentRepository;
    private readonly IEventBus _eventBus;

    public TagUseCases(
        IRepository<FileTag> tagRepository,
        IRepository<FileTagAssignment> assignmentRepository,
        IEventBus eventBus)
    {
        _tagRepository = tagRepository;
        _assignmentRepository = assignmentRepository;
        _eventBus = eventBus;
    }

    public async Task<FileTag> CreateTagAsync(string name, string color = "#808080")
    {
        var tag = new FileTag { Name = name, Color = color };
        await _tagRepository.AddAsync(tag);
        return tag;
    }

    public async Task<IEnumerable<FileTag>> GetAllTagsAsync()
    {
        return await _tagRepository.GetAllAsync();
    }

    public async Task AssignTagToFileAsync(Guid fileId, Guid tagId)
    {
        var tag = await _tagRepository.GetByIdAsync(tagId);
        if (tag is null) return;

        var allAssignments = await _assignmentRepository.GetAllAsync();
        var exists = allAssignments.Any(a => a.FileId == fileId && a.TagId == tagId);
        if (exists) return;

        var assignment = new FileTagAssignment { FileId = fileId, TagId = tagId };
        await _assignmentRepository.AddAsync(assignment);

        _eventBus.Publish(new FileTaggedEvent(fileId, tagId, tag.Name));
    }

    public async Task RemoveTagFromFileAsync(Guid fileId, Guid tagId)
    {
        var allAssignments = await _assignmentRepository.GetAllAsync();
        var assignment = allAssignments.FirstOrDefault(a => a.FileId == fileId && a.TagId == tagId);
        if (assignment is not null)
        {
            await _assignmentRepository.DeleteAsync(assignment.Id);
        }
    }

    public async Task<IEnumerable<FileTag>> GetTagsForFileAsync(Guid fileId)
    {
        var allAssignments = await _assignmentRepository.GetAllAsync();
        var tagIds = allAssignments.Where(a => a.FileId == fileId).Select(a => a.TagId);
        var allTags = await _tagRepository.GetAllAsync();
        return allTags.Where(t => tagIds.Contains(t.Id));
    }

    public async Task DeleteTagAsync(Guid tagId)
    {
        var allAssignments = await _assignmentRepository.GetAllAsync();
        var toRemove = allAssignments.Where(a => a.TagId == tagId);
        foreach (var a in toRemove)
        {
            await _assignmentRepository.DeleteAsync(a.Id);
        }
        await _tagRepository.DeleteAsync(tagId);
    }
}
'@ | Set-Content "$tagsBase/Application/UseCases/TagUseCases.cs"

# ============================================
# SEARCH MODUL
# ============================================
$searchBase = "src/Modules/Search/RicosBetterFileSearch.Modules.Search"

@'
using RicosBetterFileSearch.SharedKernel;

namespace RicosBetterFileSearch.Modules.Search.Domain.Entities;

public class SearchHistory : BaseEntity
{
    public string Query { get; set; } = string.Empty;
    public int ResultCount { get; set; }
    public string? ExtensionFilter { get; set; }
}
'@ | Set-Content "$searchBase/Domain/Entities/SearchHistory.cs"

@'
using RicosBetterFileSearch.SharedKernel;
using RicosBetterFileSearch.Modules.Search.Domain.Entities;
using RicosBetterFileSearch.Modules.Indexing.Domain.Entities;

namespace RicosBetterFileSearch.Modules.Search.Application.UseCases;

public class SearchUseCases
{
    private readonly IRepository<FileEntry> _fileRepository;
    private readonly IRepository<SearchHistory> _historyRepository;

    public SearchUseCases(
        IRepository<FileEntry> fileRepository,
        IRepository<SearchHistory> historyRepository)
    {
        _fileRepository = fileRepository;
        _historyRepository = historyRepository;
    }

    public async Task<IEnumerable<FileEntry>> SearchFilesAsync(string query, string? extensionFilter = null)
    {
        var allFiles = await _fileRepository.GetAllAsync();

        var results = allFiles.Where(f =>
            f.FileName.Contains(query, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(extensionFilter))
        {
            results = results.Where(f =>
                f.Extension.Equals(extensionFilter, StringComparison.OrdinalIgnoreCase));
        }

        var resultList = results.ToList();

        // Sacuvaj u istoriju pretrage
        var history = new SearchHistory
        {
            Query = query,
            ResultCount = resultList.Count,
            ExtensionFilter = extensionFilter
        };
        await _historyRepository.AddAsync(history);

        return resultList;
    }

    public async Task<IEnumerable<SearchHistory>> GetSearchHistoryAsync()
    {
        var all = await _historyRepository.GetAllAsync();
        return all.OrderByDescending(h => h.CreatedAt);
    }

    public async Task ClearHistoryAsync()
    {
        var all = await _historyRepository.GetAllAsync();
        foreach (var entry in all)
        {
            await _historyRepository.DeleteAsync(entry.Id);
        }
    }
}
'@ | Set-Content "$searchBase/Application/UseCases/SearchUseCases.cs"

# ============================================
# STATISTICS MODUL
# ============================================
$statsBase = "src/Modules/Statistics/RicosBetterFileSearch.Modules.Statistics"

@'
namespace RicosBetterFileSearch.Modules.Statistics.Application.DTOs;

public class StatisticsResult
{
    public int TotalFiles { get; set; }
    public int TotalFolders { get; set; }
    public long TotalSizeBytes { get; set; }
    public Dictionary<string, int> FilesByExtension { get; set; } = new();
    public Dictionary<string, int> FilesByFolder { get; set; } = new();
    public string LargestFileName { get; set; } = string.Empty;
    public long LargestFileSize { get; set; }
}
'@ | Set-Content "$statsBase/Application/DTOs/StatisticsResult.cs"

@'
using RicosBetterFileSearch.SharedKernel;
using RicosBetterFileSearch.Modules.Indexing.Domain.Entities;
using RicosBetterFileSearch.Modules.Folders.Domain.Entities;
using RicosBetterFileSearch.Modules.Statistics.Application.DTOs;

namespace RicosBetterFileSearch.Modules.Statistics.Application.UseCases;

public class StatisticsUseCases
{
    private readonly IRepository<FileEntry> _fileRepository;
    private readonly IRepository<IndexedFolder> _folderRepository;

    public StatisticsUseCases(
        IRepository<FileEntry> fileRepository,
        IRepository<IndexedFolder> folderRepository)
    {
        _fileRepository = fileRepository;
        _folderRepository = folderRepository;
    }

    public async Task<StatisticsResult> GetStatisticsAsync()
    {
        var files = (await _fileRepository.GetAllAsync()).ToList();
        var folders = (await _folderRepository.GetAllAsync()).ToList();

        var result = new StatisticsResult
        {
            TotalFiles = files.Count,
            TotalFolders = folders.Count,
            TotalSizeBytes = files.Sum(f => f.SizeInBytes),
            FilesByExtension = files
                .GroupBy(f => string.IsNullOrEmpty(f.Extension) ? "(no ext)" : f.Extension.ToLower())
                .ToDictionary(g => g.Key, g => g.Count()),
            FilesByFolder = files
                .GroupBy(f => f.FolderId)
                .ToDictionary(
                    g => folders.FirstOrDefault(fo => fo.Id == g.Key)?.FolderName ?? "Unknown",
                    g => g.Count())
        };

        var largest = files.MaxBy(f => f.SizeInBytes);
        if (largest is not null)
        {
            result.LargestFileName = largest.FileName;
            result.LargestFileSize = largest.SizeInBytes;
        }

        return result;
    }
}
'@ | Set-Content "$statsBase/Application/UseCases/StatisticsUseCases.cs"

# ============================================
# ALSO NEED: Search references Indexing (for FileEntry)
# ============================================
dotnet add src/Modules/Search/RicosBetterFileSearch.Modules.Search reference src/Modules/Folders/RicosBetterFileSearch.Modules.Folders

# BUILD
Write-Host ""
Write-Host "Building..." -ForegroundColor Yellow
dotnet build --verbosity quiet

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host " T3-T7 GOTOVO! Svi moduli dodati." -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
