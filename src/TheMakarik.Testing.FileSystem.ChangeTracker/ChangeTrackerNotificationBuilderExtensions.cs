using TheMakarik.Testing.FileSystem.ChangeTracker.Core;

namespace TheMakarik.Testing.FileSystem.ChangeTracker;

public static class ChangeTrackerNotificationBuilderExtensions
{
    public static IChangeTrackerNotificationBuilder AddLastAccessFilter(this ChangeTrackerNotificationBuilder builder)
    {
        return builder.Add(NotifyFilters.LastAccess);
    }
    
    public static IChangeTrackerNotificationBuilder AddLastWriteFilter(this ChangeTrackerNotificationBuilder builder)
    {
        return builder.Add(NotifyFilters.LastWrite);
    }
    public static IChangeTrackerNotificationBuilder AddAttributesFilter(this ChangeTrackerNotificationBuilder builder)
    {
        return builder.Add(NotifyFilters.Attributes);
    }
}