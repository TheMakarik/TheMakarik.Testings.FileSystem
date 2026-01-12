using System.IO;
using System.IO.Compression;
using TheMakarik.Testing.FileSystem.Assertion;

namespace TheMakarik.Testing.FileSystem.Zip;

public static class FileSystemExtensions
{
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