namespace TheMakarik.Testing.FileSystem.ChangeTracker.Core;

public sealed class ChangeTrackerNotificationBuilder : IChangeTrackerNotificationBuilder
{
    NotifyFilters? _filter = null;

    internal ChangeTrackerNotificationBuilder() { }
    
    public IChangeTrackerNotificationBuilder Add(NotifyFilters filter)
    {
       if (_filter is null)
           _filter = filter;
       else
           _filter |= filter;
       return this;
    }

    public NotifyFilters Build()
    {
        return _filter ?? throw  new InvalidOperationException("Cannot get notification filters because no one filters was added");
    }
}