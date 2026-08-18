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
