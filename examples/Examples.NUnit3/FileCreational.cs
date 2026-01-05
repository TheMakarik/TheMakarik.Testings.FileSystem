using TheMakarik.Testing.FileSystem;

namespace Examples.NUnit3;

public class Tests
{
    private IFileSystem _fileSystem;    
    
    [SetUp]
    public void Setup()
    {
        _fileSystem = FileSystem.BeginBuilding()
            .AddInTempRoot("MY_TEST_TEMP_FOLDER")
            .AddFile("my-test-file.txt")
            .AddFile("my-test-file2.txt", "has content")
            .AddFile("my-test-file3.txt", "has content", out var fullPath)
            .Build();
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }


    [TearDown]
    public void TearDown()
    {
        _fileSystem.Dispose();
    }
}