using TheMakarik.Testing.FileSystem.Core;
using ChangeTrackerNotificationBuilder = TheMakarik.Testing.FileSystem.ChangeTracker.Core.ChangeTrackerNotificationBuilder;
using IChangeTrackerNotificationBuilder = TheMakarik.Testing.FileSystem.ChangeTracker.Core.IChangeTrackerNotificationBuilder;

namespace TheMakarik.Testing.FileSystem.ChangeTracker;

/// <summary>
/// Provides extension methods for <see cref="IFileSystemBuilder"/> to using <see cref="ChangeTracker"/>
/// </summary>
public static class FileSystemBuilderExtensions
{
    public static IFileSystemBuilder AddChangeTracker(this IFileSystemBuilder builder)
    {
        return builder;
    }
  
    public static IFileSystemBuilder AllowChangeTrackingFor(this IFileSystemBuilder builder, string rootRelativePath)
    {
        return builder;
    }
    
    public static IFileSystemBuilder AllowChangeTrackingFor(this IFileSystemBuilder builder, string rootRelativePath, NotifyFilters notifyFilters)
    {
        return builder;
    }
    
    public static IFileSystemBuilder AllowChangeTrackingFor(this IFileSystemBuilder builder, string rootRelativePath, Func<IChangeTrackerNotificationBuilder, IChangeTrackerNotificationBuilder> configure)
    {
        return builder.AllowChangeTrackingFor(rootRelativePath, configure(new ChangeTrackerNotificationBuilder()).Build());
    }
}