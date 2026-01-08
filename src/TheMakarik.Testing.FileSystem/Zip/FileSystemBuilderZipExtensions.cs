using System;
using TheMakarik.Testing.FileSystem.Core;

namespace TheMakarik.Testing.FileSystem.Zip;

public static class FileSystemBuilderZipExtensions
{
    public static IFileSystemBuilder AddZip(this IFileSystemBuilder builder)
    {
        return builder;
    }
}