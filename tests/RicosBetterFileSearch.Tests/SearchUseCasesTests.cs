using RicosBetterFileSearch.Modules.Search.Application.UseCases;
using RicosBetterFileSearch.Modules.Search.Domain.Entities;
using RicosBetterFileSearch.Modules.Indexing.Domain.Entities;
using RicosBetterFileSearch.Modules.Tags.Domain.Entities;
using RicosBetterFileSearch.Tests.Fakes;

namespace RicosBetterFileSearch.Tests;

public class SearchUseCasesTests
{
    [Fact]
    public async Task SearchFiles_ShouldReturnMatchingFilesAndSaveHistory()
    {
        var fileRepo = new InMemoryRepository<FileEntry>();
        var historyRepo = new InMemoryRepository<SearchHistory>();
        var assignmentRepo = new InMemoryRepository<FileTagAssignment>();

        await fileRepo.AddRangeAsync(new List<FileEntry>
        {
            new() { FileName = "report.pdf", Extension = ".pdf", SizeInBytes = 1000 },
            new() { FileName = "report_v2.pdf", Extension = ".pdf", SizeInBytes = 2000 },
            new() { FileName = "photo.jpg", Extension = ".jpg", SizeInBytes = 3000 },
        });

        var sut = new SearchUseCases(fileRepo, historyRepo, assignmentRepo);
        var results = (await sut.SearchFilesAsync("report")).ToList();

        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.Contains("report", r.FileName));

        var history = (await sut.GetSearchHistoryAsync()).ToList();
        Assert.Single(history);
        Assert.Equal(2, history[0].ResultCount);
    }
}
