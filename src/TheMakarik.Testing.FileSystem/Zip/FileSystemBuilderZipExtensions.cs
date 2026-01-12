using System;
using System.IO;
using JetBrains.Annotations;
using TheMakarik.Testing.FileSystem.Core;

namespace TheMakarik.Testing.FileSystem.Zip;

/// <summary>
/// Represents the base extensions methods for adding zip archive (<see cref="IZipArchiveFileSystem"/> to your <see cref="IFileSystem"/>
/// </summary>
[PublicAPI]
public static class FileSystemBuilderZipExtensions
{
    public static IFileSystemBuilder AddZip(this IFileSystemBuilder builder, string archiveName, Func<IZipArchiveFileSystemBuilder, IZipArchiveFileSystemBuilder>? builderAction)
    {
        Guard.AgainstNull(archiveName, nameof(archiveName));
        
        archiveName = archiveName.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase) ?  archiveName : archiveName + ".zip";
     
        
        return builder.Add(archiveName, (fullPath, b) =>
        {
            var zipArchiveFileSystemBuilder = new ZipArchiveFileSystemBuilder(fullPath);
            builderAction?.Invoke(zipArchiveFileSystemBuilder).Build();
        });
    }
}