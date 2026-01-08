namespace TheMakarik.Testing.FileSystem.ChangeTracker.Core;

public interface IChangeTrackerNotificationBuilder
{
    public IChangeTrackerNotificationBuilder Add(NotifyFilters filter);
    internal NotifyFilters Build();
}