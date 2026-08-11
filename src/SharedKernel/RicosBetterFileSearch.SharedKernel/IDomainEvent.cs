namespace RicosBetterFileSearch.SharedKernel;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}