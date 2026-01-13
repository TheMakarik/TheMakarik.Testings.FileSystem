using System.IO;
using System.IO.Compression;

namespace TheMakarik.Testing.FileSystem.Zip;

/// <summary>
/// Represents the context for creating entries within a zip archive during file system construction.
/// </summary>
/// <param name="EntryName">The name of the entry being created within the zip archive.</param>
/// <param name="Archive">The zip archive instance where the entry will be created.</param>
/// <param name="Prefix">The directory prefix path within the zip archive where the entry will be located.</param>
public record ZipCreationalContext(string EntryName, ZipArchive Archive, string Prefix)
{
    /// <summary>
    /// Gets the full entry path within the zip archive, combining the prefix and entry name.
    /// </summary>
    /// <value>
    /// The full path to the entry within the zip archive, including any directory prefixes.
    /// </value>
    public string FullEntryName => Path.Combine(Prefix, EntryName);
}