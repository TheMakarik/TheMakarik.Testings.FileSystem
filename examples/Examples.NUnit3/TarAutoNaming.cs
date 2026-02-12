using TheMakarik.Testing.FileSystem;
using TheMakarik.Testing.FileSystem.AutoNaming;
using TheMakarik.Testing.FileSystem.Core;
using TheMakarik.Testing.FileSystem.SharpCompress.Tar;
using TheMakarik.Testing.FileSystem.SharpCompress.Tar.AutoNaming;

namespace Examples.NUnit3;

public class TarAutoNaming
{
    private IFileSystem _fileSystem;
    private string _tar;

    [SetUp]
    public void SetUp()
    {
        _fileSystem = FileSystem.BeginBuilding()
            .AddRandomInTempRootName()
            .AddTar("auto-naming.tar",  out _tar, tar => 
                tar.AddNameGenerator(NameGenerationType.RandomNameAndCount)
                    .AddFilesWithNameGenerating(".txt", 5, "tar auto content")
                    .AddDirectoryWithNameGenerating(inner =>
                        inner.AddFileWithNameGenerating(".log", "tar-inner-log")))
            .Build();
    }

    [Test]
    public void Tar_AutoNaming_ShouldCreateExpectedEntries()
    {
        _fileSystem.ShouldTar(_tar)
            .TotalFileCount(6); // 5 .txt + 1 .log
    }

    [TearDown]
    public void TearDown()
    {
        _fileSystem.Dispose();
    }
}

