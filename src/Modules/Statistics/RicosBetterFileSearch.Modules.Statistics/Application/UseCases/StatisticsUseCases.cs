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
                .GroupBy(f => folders.FirstOrDefault(fo => fo.Id == f.FolderId)?.FolderName ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count())
        };

        if (files.Count > 0)
        {
            var largest = files.MaxBy(f => f.SizeInBytes);
            if (largest is not null)
            {
                result.LargestFileName = largest.FileName;
                result.LargestFileSize = largest.SizeInBytes;
            }
        }

        return result;
    }
}
