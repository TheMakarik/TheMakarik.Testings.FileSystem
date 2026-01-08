using System;
using System.IO.Compression;

namespace TheMakarik.Testing.FileSystem.Zip;

public interface IZipArchiveBuilder
{
    public IZipArchiveBuilder Add(string relativePath,  Action<ZipArchive, IZipArchiveBuilder> additionalAction);
    public IZipArchiveFileSystem Build();
}