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
