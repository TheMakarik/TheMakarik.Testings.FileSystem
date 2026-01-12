using TheMakarik.Testing.FileSystem;
using TheMakarik.Testing.FileSystem.Core;
using TheMakarik.Testing.FileSystem.Zip;

namespace Examples.xUnit;

public interface IExplorer
{
    public IEnumerable<string> GetContent(string path);
}

public sealed class Explorer : IExplorer
{
    public IEnumerable<string> GetContent(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        
        if(!Directory.Exists(path))
            throw new DirectoryNotFoundException("Cannot get content from unexisting directory");
        
        return Directory.EnumerateDirectories(path).Concat(Directory.EnumerateFiles(path));
    }
}

public class ReadMeExampleTests
{
    private readonly string _mockFileFullPath;
    private readonly IFileSystem _fileSystem;
    private string _emptyDirectory;

    public ReadMeExampleTests()
    {
        _fileSystem = FileSystem.BeginBuilding()
            .AddRandomInTempRootName()
            .AddFile("mock.txt", "txt-file-with-content", out _mockFileFullPath)
            .AddDirectory("empty-directory")
            .AddDirectories(["first-directory", "second-directory"],
                (_, builder) => builder
                    .AddFile(Path.GetRandomFileName())
                    .AddFile("subdir-file.txt", "I am file from sub directory")
                    .AddFiles(["first-file", "second-file", "third-file"]))
            .AddDirectory("not-empty-directory", out _emptyDirectory)
            .AddZip("my-archive.zip", 
                builder =>  builder
                    .AddFile("README.md", "# Hello, I am my-archive.zip readme file")
                    .AddFile(Path.GetRandomFileName(), "my-archive file content"))
            .Build();
    }

    [Fact]
    public void GetContent_FromRoot_ReturnsRootContent()
    {
        //Arrange
        var systemUnderTests = new Explorer();
        //Act
        var result = systemUnderTests.GetContent(_fileSystem.Root);
        //Assert
        _fileSystem.Should()
            .ContentEquals(systemUnderTests.GetContent(_fileSystem.Root));
    }
    
    [Fact]
    public void GetContent_ReturnsContentOfTheDirectory()
    {
        //Arrange
        var systemUnderTests = new Explorer();
        //Act
        var result = systemUnderTests.GetContent(_emptyDirectory);
        //Assert
        _fileSystem
            .In(_emptyDirectory)
            .Should()
            .HasNoDirectoryContent()
            .ContentEquals(systemUnderTests.GetContent(_fileSystem.Root));
    }

}