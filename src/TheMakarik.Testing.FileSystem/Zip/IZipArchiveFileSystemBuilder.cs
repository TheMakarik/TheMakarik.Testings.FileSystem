using System;
using System.IO.Compression;
using JetBrains.Annotations;

namespace TheMakarik.Testing.FileSystem.Zip;

public interface IZipArchiveFileSystemBuilder
{
    public string Root { get; }
    public IZipArchiveFileSystemBuilder Add(string relativePath,  Action<ZipArchive, string> additionalAction);
    [PublicAPI]
    public IZipArchiveFileSystem Build();
}