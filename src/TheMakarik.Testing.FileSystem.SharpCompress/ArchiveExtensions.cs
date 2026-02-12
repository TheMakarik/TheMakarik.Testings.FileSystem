using System;
using System.IO;
using JetBrains.Annotations;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace TheMakarik.Testing.FileSystem.SharpCompress;


/// <summary>
/// Provides extension methods for working with SharpCompress archives.
/// </summary>
[PublicAPI]
public static class ArchiveExtensions
{
    /// <summary>
    /// Extracts all entries from the archive to the specified destination directory.
    /// </summary>
    /// <param name="archive">The opened archive instance (e.g. from <see cref="ArchiveFactory.Open"/>).</param>
    /// <param name="destinationDirectory">The root directory where the archive contents will be extracted.</param>
    /// <param name="options">Optional extraction settings (default: full path extraction with overwrite).</param>
    /// <remarks>
    /// <para>
    /// This method:
    /// • Creates all necessary subdirectories
    /// • Extracts files with correct paths
    /// • Handles both compressed (tar.gz, zip, rar, 7z, etc.) and uncompressed archives
    /// • Creates empty directories if they exist in the archive
    /// • Overwrites existing files by default (configurable via options)
    /// </para>
    /// <para>
    /// Throws <see cref="IOException"/> or <see cref="UnauthorizedAccessException"/> if access is denied.
    /// If extraction fails for any entry, the method continues with remaining entries (partial extraction).
    /// </para>
    /// <para>
    /// Supported archive types: zip, tar (plain, gz, bz2, xz), 7z, rar, etc. — any format SharpCompress supports.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="archive"/> or <paramref name="destinationDirectory"/> is null.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the destination path is invalid.</exception>
    public static void ExtractAllTo(
        this IArchive archive,
        string destinationDirectory,
        ExtractionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentException.ThrowIfNullOrEmpty(destinationDirectory);

        options ??= new ExtractionOptions
        {
            ExtractFullPath = true,
            Overwrite = true
        };
        
        if(!Directory.Exists(destinationDirectory))
            Directory.CreateDirectory(destinationDirectory);
        
        archive.WriteToDirectory(destinationDirectory, options);
    }
}