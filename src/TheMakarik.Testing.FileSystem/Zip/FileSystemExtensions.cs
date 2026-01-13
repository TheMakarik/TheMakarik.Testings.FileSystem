using System.IO;
using System.IO.Compression;
using TheMakarik.Testing.FileSystem.Assertion;

namespace TheMakarik.Testing.FileSystem.Zip;

/// <summary>
/// Provides extension methods for validating zip archive contents within file system assertions.
/// </summary>
public static class FileSystemExtensions
{
    /// <summary>
    /// Creates an assertion context for validating the contents of a zip archive within the file system.
    /// </summary>
    /// <param name="fileSystem">The file system instance containing the zip archive.</param>
    /// <param name="rootRelativeZipArchiveName">The relative path to the zip archive from the root directory.</param>
    /// <returns>An <see cref="IFileSystemAssertion"/> instance for validating the extracted zip archive contents.</returns>
    /// <remarks>
    /// This method extracts the specified zip archive to a temporary directory and returns an assertion context
    /// that operates on the extracted contents. All subsequent assertion methods will validate the files and directories
    /// extracted from the zip archive.
    /// </remarks>
    public static IFileSystemAssertion ShouldZip(this IFileSystem fileSystem, string rootRelativeZipArchiveName)
    {
        using var zip = ZipFile.OpenRead(Path.Combine(fileSystem.Root, rootRelativeZipArchiveName));
        var outputDirectory = GetRandomTempDirectory();
        zip.ExtractToDirectory(outputDirectory);
       
        var extractedZipFileSystem = new FileSystem(outputDirectory);
        
        fileSystem.Disposed += (_, _) => extractedZipFileSystem.Dispose();
        
        return new FileSystemAssertion(extractedZipFileSystem);
    }
    
    private static string GetRandomTempDirectory()
    {
        return Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
    }
}