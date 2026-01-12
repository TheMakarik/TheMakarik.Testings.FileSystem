using System.IO;
using System.IO.Compression;

namespace TheMakarik.Testing.FileSystem.Zip;

public static class ZipArchiveBuilderExtensions
{
    public static IZipArchiveFileSystemBuilder AddFile(this IZipArchiveFileSystemBuilder builder, string fileName, CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        return builder.Add(fileName, (archive, archivePath) =>
        {
            archive.CreateEntry(fileName, compressionLevel);
        });
    }
    
    public static IZipArchiveFileSystemBuilder AddFile(this IZipArchiveFileSystemBuilder builder, string fileName, string content, CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        return builder.Add(fileName, (archive, archivePath) =>
        {
            var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            using (var stream = File.Create(tempFilePath))
            {
                using var streamWriter = new StreamWriter(stream);
                streamWriter.Write(content);
            }
            
            archive.CreateEntryFromFile(tempFilePath, fileName, compressionLevel);
            File.Delete(tempFilePath);
        });
    }
    
}