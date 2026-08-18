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
