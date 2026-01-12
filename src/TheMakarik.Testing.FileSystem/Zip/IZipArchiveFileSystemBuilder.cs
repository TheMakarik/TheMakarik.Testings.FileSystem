using System;
using System.IO.Compression;
using JetBrains.Annotations;

namespace TheMakarik.Testing.FileSystem.Zip;

[PublicAPI]
public interface IZipArchiveFileSystemBuilder
{
    public string Root { get; }
    
    /// <summary>
    /// Archive prefix, every entry name will be started from it, use it for directories
    /// </summary>
    public string Prefix { get; }
    public IZipArchiveFileSystemBuilder Add(string relativePath,  Action<ZipArchive, string> additionalAction);
    
    public void Build();
}