using System;
using TheMakarik.Testing.FileSystem.Core.Events;

namespace TheMakarik.Testing.FileSystem.Core;

public interface IDefaultFileSystemBuilderEvents
{
    public EventHandler<ElementAddedEventArgs> Added { get; set; }
}