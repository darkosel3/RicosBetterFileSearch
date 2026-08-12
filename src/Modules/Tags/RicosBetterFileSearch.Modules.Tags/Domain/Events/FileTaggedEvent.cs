using RicosBetterFileSearch.SharedKernel;

namespace RicosBetterFileSearch.Modules.Tags.Domain.Events;

public class FileTaggedEvent : IDomainEvent
{
    public Guid FileId { get; }
    public Guid TagId { get; }
    public string TagName { get; }
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public FileTaggedEvent(Guid fileId, Guid tagId, string tagName)
    {
        FileId = fileId;
        TagId = tagId;
        TagName = tagName;
    }
}
