using TheMakarik.Testing.FileSystem;

namespace Examples.NUnit3;

public class AddingDirectoryWithContent
{
    private IFileSystem _fileSystem;    
    
    [SetUp]
    public void Setup()
    {
        _fileSystem = FileSystem.BeginBuilding()
            .AddInTempRoot("MY_TEST_TEMP_FOLDER")
            .AddDirectory("test-folder", (_, builder) => builder
                    .AddFile("test-file.txt")
                    .AddDirectory("test-subdir", (_, builder) => builder
                        .AddFile("hello.txt")
                        .AddDirectory("directory-without-content"))
                    .AddFile("test-file2.txt"))  
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