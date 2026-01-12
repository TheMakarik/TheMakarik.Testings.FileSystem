using System.IO.Compression;
using TheMakarik.Testing.FileSystem;
using TheMakarik.Testing.FileSystem.Core;
using TheMakarik.Testing.FileSystem.Zip;

namespace Examples.xUnit;

public class ZipFileAssertions : IDisposable
{
    private readonly IFileSystem _fileSystem;
    private readonly string _zipPath;

    public ZipFileAssertions()
    {
        _fileSystem = FileSystem.BeginBuilding()
            .AddRandomInTempRootName()
            .AddZip("test-zip", out _zipPath, b => b
                .AddFile("tests/zip.txt", "txt content", CompressionLevel.Fastest)
                .AddFile("tests/zip2.txt", "txt content", CompressionLevel.Optimal))
            .Build();
    }

    [Fact]
    public void ZipContentEqualsTo()
    {
        _fileSystem.ShouldZip("test-zip.zip")
            .DirectoryHasFileCount("tests", 2);
    }

    [Fact]
    public void ZipFile_ShouldContainBothFiles()
    {
        _fileSystem.ShouldZip("test-zip.zip")
            .FileExists("tests/zip.txt")
            .FileExists("tests/zip2.txt")
            .AllFilesInDirectorySatisfy("tests", 
                fileName => fileName.EndsWith(".txt"));
    }

    [Fact]
    public void ZipFile_ShouldHaveCorrectContent()
    {
        _fileSystem.ShouldZip("test-zip.zip")
            .FileContentEquals("tests/zip.txt", "txt content")
            .FileContentEquals("tests/zip2.txt", "txt content");
    }

    public void Dispose()
    {
        _fileSystem.Dispose();
    }
}