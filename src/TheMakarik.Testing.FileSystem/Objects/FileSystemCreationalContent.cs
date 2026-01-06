using System;
using System.IO;
using TheMakarik.Testing.FileSystem.Core;

namespace TheMakarik.Testing.FileSystem.Objects;

/// <summary>
/// Encapsulate action for creating element at the  <see cref="FileSystem"/>, until <see cref="IFileSystemBuilder.Build"/> will not be called. This class is sealed
/// </summary>
/// <param name="buildingAction">Action to build</param>
/// <param name="rootRelativePath">Relative to root directory path</param>
internal sealed class FileSystemCreationalContent(Action<string, IFileSystemBuilder> buildingAction, string rootRelativePath)
{
    /// <summary>
    /// Invokes encapsulated action to build the <see cref="FileSystem"/>
    /// </summary>
    /// <param name="fileSystemBuilder"> <see cref="FileSystem"/> builder</param>
    internal void InvokeBuildingAction(IFileSystemBuilder fileSystemBuilder)
    {
        buildingAction(Path.Combine(fileSystemBuilder.RootDirectory, rootRelativePath), fileSystemBuilder);
    }
}