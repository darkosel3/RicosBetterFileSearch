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
