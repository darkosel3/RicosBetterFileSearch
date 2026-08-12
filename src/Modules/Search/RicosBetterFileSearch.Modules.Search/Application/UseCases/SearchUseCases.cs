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
