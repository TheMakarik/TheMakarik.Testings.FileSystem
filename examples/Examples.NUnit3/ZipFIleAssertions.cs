using System.IO.Compression;
using TheMakarik.Testing.FileSystem;
using TheMakarik.Testing.FileSystem.Core;
using TheMakarik.Testing.FileSystem.Zip;

namespace Examples.NUnit3;

public class ZipFIleAssertions
{
    private IFileSystem _fileSystem;
    private string _zipPath;

    [SetUp]
    public void SetUp()
    {
        _fileSystem = FileSystem.BeginBuilding()
            .AddRandomInTempRootName()
            .AddZip("test-zip", out _zipPath, (b) => b
                .AddFile("tests/zip.txt", "txt content", CompressionLevel.Fastest)
                .AddFile("tests/zip2.txt", "txt content", CompressionLevel.Optimal))
            .Build();
    }

    [Test]
    public void ZipContentEqualsTo()
    {
        _fileSystem.ShouldZip(_zipPath)
            .DirectoryHasFileCount("tests", 2);
    }
    
    [TearDown]
    public void TearDown()
    {
        _fileSystem.Dispose();
    }
}