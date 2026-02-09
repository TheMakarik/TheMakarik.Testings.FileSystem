using TheMakarik.Testing.FileSystem;
using TheMakarik.Testing.FileSystem.AutoNaming;
using TheMakarik.Testing.FileSystem.Core;
using TheMakarik.Testing.FileSystem.Zip;
using TheMakarik.Testing.FileSystem.Zip.AutoNaming;

namespace Examples.xUnit;

public class ZipAutoNaming : IDisposable
{
    private readonly IFileSystem _fileSystem;
    private readonly string _zipPath;

    public ZipAutoNaming()
    {
        _fileSystem = FileSystem.BeginBuilding()
            .AddRandomInTempRootName()
            .AddZip("auto-naming.zip", out _zipPath, zip => zip
                .AddNameGenerator(NameGenerationType.RandomNameAndCount)
                .AddFilesWithNameGenerating(".txt", 10, "zip auto content")
                .AddDirectoryWithNameGenerating(inner =>
                    inner.AddFileWithNameGenerating(".log", "zip-inner-log")))
            .Build();
    }

    [Fact]
    public void Zip_AutoNaming_ShouldCreateExpectedFiles()
    {
        _fileSystem.ShouldZip(_zipPath)
            .TotalFileCount(11); // 10 .txt + 1 .log
    }

    public void Dispose()
    {
        _fileSystem.Dispose();
    }
}

