using RicosBetterFileSearch.Modules.Statistics.Application.UseCases;
using RicosBetterFileSearch.Modules.Indexing.Domain.Entities;
using RicosBetterFileSearch.Modules.Folders.Domain.Entities;
using RicosBetterFileSearch.Tests.Fakes;

namespace RicosBetterFileSearch.Tests;

public class StatisticsUseCasesTests
{
    [Fact]
    public async Task GetStatistics_ShouldAggregateCorrectly()
    {
        var fileRepo = new InMemoryRepository<FileEntry>();
        var folderRepo = new InMemoryRepository<IndexedFolder>();

        var folder = new IndexedFolder { FolderPath = @"C:\Test", FolderName = "Test" };
        await folderRepo.AddAsync(folder);

        await fileRepo.AddRangeAsync(new List<FileEntry>
        {
            new() { FileName = "a.txt", Extension = ".txt", SizeInBytes = 100, FolderId = folder.Id },
            new() { FileName = "b.txt", Extension = ".txt", SizeInBytes = 200, FolderId = folder.Id },
            new() { FileName = "c.pdf", Extension = ".pdf", SizeInBytes = 5000, FolderId = folder.Id },
        });

        var sut = new StatisticsUseCases(fileRepo, folderRepo);
        var stats = await sut.GetStatisticsAsync();

        Assert.Equal(3, stats.TotalFiles);
        Assert.Equal(1, stats.TotalFolders);
        Assert.Equal(5300, stats.TotalSizeBytes);
        Assert.Equal(2, stats.FilesByExtension[".txt"]);
        Assert.Equal(1, stats.FilesByExtension[".pdf"]);
        Assert.Equal("c.pdf", stats.LargestFileName);
    }
}
