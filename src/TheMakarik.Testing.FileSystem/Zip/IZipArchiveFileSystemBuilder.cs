using System;
using System.IO.Compression;
using JetBrains.Annotations;

namespace TheMakarik.Testing.FileSystem.Zip;

/// <summary>
/// Represents a builder for creating and configuring zip archives within a file system.
/// </summary>
[PublicAPI]
public interface IZipArchiveFileSystemBuilder
{
    /// <summary>
    /// Gets the full path to the zip archive file.
    /// </summary>
    public string Root { get; }
    
    /// <summary>
    /// Gets the current directory prefix within the zip archive.
    /// All entry names will be prefixed with this path when added to the archive.
    /// </summary>
    /// <remarks>
    /// Use this property to work with nested directory structures within the zip archive.
    /// </remarks>
    public string Prefix { get; }
    
    /// <summary>
    /// Adds a new entry to the zip archive at the specified relative path.
    /// </summary>
    /// <param name="relativePath">The relative path of the entry within the zip archive.</param>
    /// <param name="additionalAction">The action that creates the entry content, receiving a <see cref="ZipCreationalContext"/> with creation parameters.</param>
    /// <returns>The same <see cref="IZipArchiveFileSystemBuilder"/> instance for method chaining.</returns>
    public IZipArchiveFileSystemBuilder Add(string relativePath, Action<ZipCreationalContext> additionalAction);
    
    /// <summary>
    /// Finalizes the zip archive creation process and writes all configured entries to the archive file.
    /// </summary>
    /// <remarks>
    /// This method must be called to actually create the zip archive with all configured entries.
    /// </remarks>
    public void Build();
}