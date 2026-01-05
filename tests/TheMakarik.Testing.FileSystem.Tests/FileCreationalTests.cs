using FluentAssertions;
using TheMakarik.Testing.FileSystem.Core;

namespace TheMakarik.Testing.FileSystem.Tests;

public class FileCreationalTests : IDisposable
{
    private IFileSystem _systemUnderTests;
    private string _root;
    private string _contentFile;

    public FileCreationalTests()
    {
        _systemUnderTests = FileSystem.BeginBuilding()
            .AddInTempRoot("root-directory", out _root)
            .AddFile("test-file.txt", "has a content", out _contentFile)
            .Build();
        
    }
    
    [Fact]
    public void ContnetFile_MustHaveContent()
    {
       //Arrange
       var content = File.ReadAllText(_contentFile);
       //Act
       var result = content.Length > 0;
       //Assert
       result.Should().BeTrue();
    }

    public void Dispose()
    {
        _systemUnderTests.Dispose();
    }
}