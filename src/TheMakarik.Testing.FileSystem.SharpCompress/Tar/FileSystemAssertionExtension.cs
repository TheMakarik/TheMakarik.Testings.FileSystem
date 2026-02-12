using System.IO;
using System.Linq;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using TheMakarik.Testing.FileSystem.Assertion;

namespace TheMakarik.Testing.FileSystem.SharpCompress.Tar;

/// <summary>
/// Extension methods for <see cref="IFileSystem"/> to assert on tar archives.
/// </summary>
public static class FileSystemAssertionExtensions
{
    /// <summary>
    /// Creates an assertion context for a tar archive by extracting it to temp.
    /// </summary>
    /// <param name="fileSystem">The file system containing the tar.</param>
    /// <param name="rootRelativeTarArchiveName">Relative path to the tar file.</param>
    /// <returns>Assertion on the extracted content.</returns>
    /// <remarks>
    /// Extracts to a temp directory, which is cleaned on dispose.
    /// Supports compressed tar (.tar.gz, .tar.bz2) via auto-detection.
    /// </remarks>
    public static IFileSystemAssertion ShouldTar(this IFileSystem fileSystem, string rootRelativeTarArchiveName)
    {
        var tarPath = Path.Combine(fileSystem.Root, rootRelativeTarArchiveName);
        var outputDirectory = Path.Combine(Path.GetTempPath(),
            Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));

        using var archive = TarArchive.Open(tarPath);
        archive.ExtractAllTo(outputDirectory);
        
        var system = new FileSystem(outputDirectory);
        return system.Should();
    }
}