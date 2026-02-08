using System;

namespace TheMakarik.Testing.FileSystem.Core.Events;

/// <summary>
/// Default args for <see cref="IDefaultFileSystemBuilderEvents.Added"/> event
/// </summary>
public class ElementAddedEventArgs : EventArgs
{
    /// <summary>
    /// FullPath (or archive relative) to element that was added
    /// </summary>
    public string FullPath { get; init; }
}