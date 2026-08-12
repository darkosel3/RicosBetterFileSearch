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
