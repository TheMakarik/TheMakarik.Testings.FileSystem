using TheMakarik.Testing.FileSystem;

namespace Examples.NUnit3;

public class AssertionExamples
{
    private IFileSystem _fileSystem;
    private string[] _emptyFilePaths;
    private string _directory1;

    [SetUp]
    public void Setup()
    {
        _fileSystem = FileSystem.BeginBuilding()
            .AddRandomInTempRootName()
            .AddFiles(["file-1", "file-2", "file-3"], "content", out _emptyFilePaths)
            .AddDirectory("dir-1", out _directory1)
            .Build();
    }

    [Test]
    public void File1_MustHaveContent()
    {
        _fileSystem.Should()
            .FileContentEquals("file-1", "content");
    }

    [Test]
    public void Directory1_MustHaveNoContent()
    {
        _fileSystem
            .Should()
            .HasNoDirectoryContent(_directory1);
    }
    


    [TearDown]
    public void TearDown()
    {
        _fileSystem.Dispose();
    }
}