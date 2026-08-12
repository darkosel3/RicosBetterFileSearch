using RicosBetterFileSearch.SharedKernel;

namespace RicosBetterFileSearch.Modules.Tags.Domain.Entities;

public class FileTagAssignment : BaseEntity
{
    public Guid FileId { get; set; }
    public Guid TagId { get; set; }
}
