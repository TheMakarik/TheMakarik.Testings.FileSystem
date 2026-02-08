using System;
using System.Collections.Generic;

namespace TheMakarik.Testing.FileSystem.SharpCompress.Tar;

/// <summary>
/// Represents a builder for creating and configuring tar archives within a file system.
/// </summary>
/// <remarks>
/// This interface provides a fluent API for adding entries to a tar archive.
/// The archive can be optionally compressed using GZip or BZip2.
/// </remarks>
public interface ITarFileSystemBuilder
{
    /// <summary>
    /// Gets the full path to the tar archive file.
    /// </summary>
    string Root { get; }

    /// <summary>
    /// Gets the dictionary for storing dynamic properties for extensibility.
    /// </summary>
    Dictionary<object, object> Properties { get; }

    /// <summary>
    /// Gets the current directory prefix within the tar archive.
    /// All entry names will be prefixed with this path when added to the archive.
    /// </summary>
    /// <remarks>
    /// Use this to create nested directory structures within the tar archive.
    /// Paths use forward slashes (/) regardless of the operating system.
    /// </remarks>
    string Prefix { get; }

    /// <summary>
    /// Adds a new entry to the tar archive at the specified relative path.
    /// </summary>
    /// <param name="relativePath">The relative path of the entry within the tar archive.</param>
    /// <param name="additionalAction">The action that creates the entry content, receiving a <see cref="TarCreationalContext"/>.</param>
    /// <returns>The same builder instance for fluent chaining.</returns>
    ITarFileSystemBuilder Add(string relativePath, Action<TarCreationalContext> additionalAction);

    /// <summary>
    /// Finalizes the tar archive creation and writes all configured entries to the file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method must be called to create the actual tar file.
    /// If an exception occurs during building, any partially created file is deleted.
    /// </para>
    /// <para>
    /// Compression (if specified) is applied to the entire archive.
    /// </para>
    /// </remarks>
    void Build();
}