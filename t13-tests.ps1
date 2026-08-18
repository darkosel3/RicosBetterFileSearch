# T13: Unit testovi
# Pokreni iz C:\Users\Darko\source\repos\RicosBetterFileSearch

$testsBase = "tests/RicosBetterFileSearch.Tests"

# Obrisi default test fajl
Remove-Item "$testsBase/UnitTest1.cs" -ErrorAction SilentlyContinue

# ============================================
# InMemoryRepository - fake za testove
# ============================================
@'
using RicosBetterFileSearch.SharedKernel;

namespace RicosBetterFileSearch.Tests.Fakes;

public class InMemoryRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly List<T> _items = new();

    public Task<T?> GetByIdAsync(Guid id) =>
        Task.FromResult(_items.FirstOrDefault(x => x.Id == id));

    public Task<IEnumerable<T>> GetAllAsync() =>
        Task.FromResult<IEnumerable<T>>(_items.ToList());

    public Task AddAsync(T entity)
    {
        _items.Add(entity);
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<T> entities)
    {
        _items.AddRange(entities);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity)
    {
        var index = _items.FindIndex(x => x.Id == entity.Id);
        if (index >= 0) _items[index] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _items.RemoveAll(x => x.Id == id);
        return Task.CompletedTask;
    }

    public Task DeleteWhereAsync(Func<T, bool> predicate)
    {
        _items.RemoveAll(x => predicate(x));
        return Task.CompletedTask;
    }
}
'@ | Set-Content "$testsBase/Fakes/InMemoryRepository.cs"

# Kreiraj Fakes folder ako ne postoji
mkdir "$testsBase/Fakes" -Force | Out-Null

# ponovo posto mkdir moze da pregazi
@'
using RicosBetterFileSearch.SharedKernel;

namespace RicosBetterFileSearch.Tests.Fakes;

public class InMemoryRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly List<T> _items = new();

    public Task<T?> GetByIdAsync(Guid id) =>
        Task.FromResult(_items.FirstOrDefault(x => x.Id == id));

    public Task<IEnumerable<T>> GetAllAsync() =>
        Task.FromResult<IEnumerable<T>>(_items.ToList());

    public Task AddAsync(T entity)
    {
        _items.Add(entity);
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<T> entities)
    {
        _items.AddRange(entities);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity)
    {
        var index = _items.FindIndex(x => x.Id == entity.Id);
        if (index >= 0) _items[index] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _items.RemoveAll(x => x.Id == id);
        return Task.CompletedTask;
    }

    public Task DeleteWhereAsync(Func<T, bool> predicate)
    {
        _items.RemoveAll(x => predicate(x));
        return Task.CompletedTask;
    }
}
'@ | Set-Content "$testsBase/Fakes/InMemoryRepository.cs"

# ============================================
# FakeFileSystemService - za testove
# ============================================
@'
using RicosBetterFileSearch.Modules.Indexing.Application.Ports;
using RicosBetterFileSearch.Modules.Indexing.Domain.Entities;

namespace RicosBetterFileSearch.Tests.Fakes;

public class TestFileSystemService : IFileSystemService
{
    private readonly List<FileEntry> _files;

    public TestFileSystemService(List<FileEntry>? files = null)
    {
        _files = files ?? new List<FileEntry>
        {
            new() { FileName = "test.txt", FilePath = "/fake/test.txt", Extension = ".txt", SizeInBytes = 100 },
            new() { FileName = "photo.jpg", FilePath = "/fake/photo.jpg", Extension = ".jpg", SizeInBytes = 2048 },
            new() { FileName = "doc.pdf", FilePath = "/fake/doc.pdf", Extension = ".pdf", SizeInBytes = 5000 },
        };
    }

    public Task<IEnumerable<FileEntry>> ScanFolderAsync(string folderPath, Guid folderId)
    {
        foreach (var f in _files) f.FolderId = folderId;
        return Task.FromResult<IEnumerable<FileEntry>>(_files);
    }
}
'@ | Set-Content "$testsBase/Fakes/TestFileSystemService.cs"

# ============================================
# TEST 1+2: FolderUseCases
# ============================================
@'
using RicosBetterFileSearch.Modules.Folders.Application.UseCases;
using RicosBetterFileSearch.Modules.Folders.Domain.Entities;
using RicosBetterFileSearch.Tests.Fakes;

namespace RicosBetterFileSearch.Tests;

public class FolderUseCasesTests
{
    private readonly FolderUseCases _sut;
    private readonly InMemoryRepository<IndexedFolder> _repo;

    public FolderUseCasesTests()
    {
        _repo = new InMemoryRepository<IndexedFolder>();
        _sut = new FolderUseCases(_repo);
    }

    [Fact]
    public async Task AddFolder_ShouldCreateAndReturnFolder()
    {
        var folder = await _sut.AddFolderAsync(@"C:\TestFolder");

        Assert.NotNull(folder);
        Assert.Equal(@"C:\TestFolder", folder.FolderPath);
        Assert.Equal("TestFolder", folder.FolderName);
        Assert.True(folder.IsActive);
    }

    [Fact]
    public async Task RemoveFolder_ShouldDeleteFromRepository()
    {
        var folder = await _sut.AddFolderAsync(@"C:\ToDelete");
        await _sut.RemoveFolderAsync(folder.Id);

        var all = await _sut.GetAllFoldersAsync();
        Assert.Empty(all);
    }
}
'@ | Set-Content "$testsBase/FolderUseCasesTests.cs"

# ============================================
# TEST 3: IndexingUseCases
# ============================================
@'
using RicosBetterFileSearch.SharedKernel;
using RicosBetterFileSearch.Modules.Folders.Domain.Entities;
using RicosBetterFileSearch.Modules.Indexing.Application.UseCases;
using RicosBetterFileSearch.Modules.Indexing.Domain.Entities;
using RicosBetterFileSearch.Tests.Fakes;

namespace RicosBetterFileSearch.Tests;

public class IndexingUseCasesTests
{
    [Fact]
    public async Task ScanFolder_ShouldIndexFilesAndPublishEvent()
    {
        var folderRepo = new InMemoryRepository<IndexedFolder>();
        var fileRepo = new InMemoryRepository<FileEntry>();
        var fakeFs = new TestFileSystemService();
        var eventBus = new InMemoryEventBus();
        var eventFired = false;

        eventBus.Subscribe<Modules.Folders.Domain.Events.FolderScannedEvent>(e => eventFired = true);

        var sut = new IndexingUseCases(fileRepo, folderRepo, fakeFs, eventBus);

        var folder = new IndexedFolder { FolderPath = @"C:\Fake", FolderName = "Fake", IsActive = true };
        await folderRepo.AddAsync(folder);

        var count = await sut.ScanFolderAsync(folder.Id);

        Assert.Equal(3, count);
        Assert.True(eventFired);

        var files = await fileRepo.GetAllAsync();
        Assert.Equal(3, files.Count());
    }
}
'@ | Set-Content "$testsBase/IndexingUseCasesTests.cs"

# ============================================
# TEST 4: SearchUseCases
# ============================================
@'
using RicosBetterFileSearch.Modules.Search.Application.UseCases;
using RicosBetterFileSearch.Modules.Search.Domain.Entities;
using RicosBetterFileSearch.Modules.Indexing.Domain.Entities;
using RicosBetterFileSearch.Tests.Fakes;

namespace RicosBetterFileSearch.Tests;

public class SearchUseCasesTests
{
    [Fact]
    public async Task SearchFiles_ShouldReturnMatchingFilesAndSaveHistory()
    {
        var fileRepo = new InMemoryRepository<FileEntry>();
        var historyRepo = new InMemoryRepository<SearchHistory>();

        await fileRepo.AddRangeAsync(new List<FileEntry>
        {
            new() { FileName = "report.pdf", Extension = ".pdf", SizeInBytes = 1000 },
            new() { FileName = "report_v2.pdf", Extension = ".pdf", SizeInBytes = 2000 },
            new() { FileName = "photo.jpg", Extension = ".jpg", SizeInBytes = 3000 },
        });

        var sut = new SearchUseCases(fileRepo, historyRepo);
        var results = (await sut.SearchFilesAsync("report")).ToList();

        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.Contains("report", r.FileName));

        var history = (await sut.GetSearchHistoryAsync()).ToList();
        Assert.Single(history);
        Assert.Equal("report", history[0].Query);
        Assert.Equal(2, history[0].ResultCount);
    }
}
'@ | Set-Content "$testsBase/SearchUseCasesTests.cs"

# ============================================
# TEST 5: TagUseCases
# ============================================
@'
using RicosBetterFileSearch.SharedKernel;
using RicosBetterFileSearch.Modules.Tags.Application.UseCases;
using RicosBetterFileSearch.Modules.Tags.Domain.Entities;
using RicosBetterFileSearch.Tests.Fakes;

namespace RicosBetterFileSearch.Tests;

public class TagUseCasesTests
{
    [Fact]
    public async Task AssignTag_ShouldLinkTagToFile()
    {
        var tagRepo = new InMemoryRepository<FileTag>();
        var assignmentRepo = new InMemoryRepository<FileTagAssignment>();
        var eventBus = new InMemoryEventBus();

        var sut = new TagUseCases(tagRepo, assignmentRepo, eventBus);

        var tag = await sut.CreateTagAsync("Important", "#ff0000");
        var fileId = Guid.NewGuid();

        await sut.AssignTagToFileAsync(fileId, tag.Id);

        var tags = (await sut.GetTagsForFileAsync(fileId)).ToList();
        Assert.Single(tags);
        Assert.Equal("Important", tags[0].Name);
    }
}
'@ | Set-Content "$testsBase/TagUseCasesTests.cs"

# ============================================
# TEST 6: StatisticsUseCases
# ============================================
@'
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
'@ | Set-Content "$testsBase/StatisticsUseCasesTests.cs"

# ============================================
# TEST 7: Event propagation
# ============================================
@'
using RicosBetterFileSearch.SharedKernel;
using RicosBetterFileSearch.Modules.Folders.Domain.Events;
using RicosBetterFileSearch.Modules.Tags.Domain.Events;

namespace RicosBetterFileSearch.Tests;

public class EventBusTests
{
    [Fact]
    public void Publish_ShouldNotifySubscribers()
    {
        var bus = new InMemoryEventBus();
        FolderScannedEvent? received = null;

        bus.Subscribe<FolderScannedEvent>(e => received = e);
        bus.Publish(new FolderScannedEvent(Guid.NewGuid(), @"C:\Test", 42));

        Assert.NotNull(received);
        Assert.Equal(42, received.FilesFound);
    }

    [Fact]
    public void Publish_ShouldNotNotifyWrongSubscribers()
    {
        var bus = new InMemoryEventBus();
        var wrongFired = false;

        bus.Subscribe<FileTaggedEvent>(e => wrongFired = true);
        bus.Publish(new FolderScannedEvent(Guid.NewGuid(), @"C:\Test", 10));

        Assert.False(wrongFired);
    }
}
'@ | Set-Content "$testsBase/EventBusTests.cs"

# RUN TESTS
Write-Host ""
Write-Host "Building and running tests..." -ForegroundColor Yellow
dotnet test --verbosity normal

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host " T13 GOTOVO! 8 testova." -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
