using System.IO;
using SharpCompress.Archives.Tar;
using SharpCompress.Writers;
using SharpCompress.Writers.Tar;

namespace TheMakarik.Testing.FileSystem.SharpCompress.Tar;

/// <summary>
/// Represents the context for creating entries within a tar archive during file system construction.
/// </summary>
/// <param name="EntryName">The name of the entry being created within the tar archive.</param>
/// <param name="Archive">The tar archive instance where the entry will be created.</param>
/// <param name="Prefix">The directory prefix path within the tar archive where the entry will be located.</param>
public record TarCreationalContext(string EntryName, IWriter Archive, string Prefix)
{
    /// <summary>
    /// Gets the full entry path within the tar archive, combining the prefix and entry name.
    /// </summary>
    /// <value>
    /// The full path to the entry within the tar archive, including any directory prefixes.
    /// </value>
    public string FullEntryName => Path.Combine(Prefix, EntryName).Replace("\\", "/");
}