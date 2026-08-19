using RicosBetterFileSearch.SharedKernel;
using RicosBetterFileSearch.Modules.Search.Domain.Entities;
using RicosBetterFileSearch.Modules.Indexing.Domain.Entities;
using RicosBetterFileSearch.Modules.Tags.Domain.Entities;

namespace RicosBetterFileSearch.Modules.Search.Application.UseCases;

public class SearchUseCases
{
    private readonly IRepository<FileEntry> _fileRepository;
    private readonly IRepository<SearchHistory> _historyRepository;
    private readonly IRepository<FileTagAssignment> _assignmentRepository;

    public SearchUseCases(
        IRepository<FileEntry> fileRepository,
        IRepository<SearchHistory> historyRepository,
        IRepository<FileTagAssignment> assignmentRepository)
    {
        _fileRepository = fileRepository;
        _historyRepository = historyRepository;
        _assignmentRepository = assignmentRepository;
    }

    public async Task<IEnumerable<FileEntry>> SearchFilesAsync(string? query, string? extensionFilter = null, Guid? tagId = null)
    {
        var allFiles = await _fileRepository.GetAllAsync();
        var results = allFiles.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            results = results.Where(f =>
                f.FileName.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(extensionFilter))
        {
            var ext = extensionFilter.StartsWith('.') ? extensionFilter : $".{extensionFilter}";
            results = results.Where(f =>
                f.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase));
        }

        if (tagId.HasValue)
        {
            var assignments = await _assignmentRepository.GetAllAsync();
            var fileIds = assignments.Where(a => a.TagId == tagId.Value).Select(a => a.FileId).ToHashSet();
            results = results.Where(f => fileIds.Contains(f.Id));
        }

        var resultList = results.ToList();

        var historyQuery = string.Join(" | ", new[] { query, extensionFilter, tagId?.ToString() }.Where(x => !string.IsNullOrEmpty(x)));
        if (!string.IsNullOrWhiteSpace(historyQuery))
        {
            var history = new SearchHistory
            {
                Query = historyQuery,
                ResultCount = resultList.Count,
                ExtensionFilter = extensionFilter
            };
            await _historyRepository.AddAsync(history);
        }

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
