using AutoFixture.Xunit2;
using FluentAssertions;

namespace TheMakarik.Testing.FileSystem.Tests;

public class FileCreationalTests
{
    [Fact]
    public void AddFile_AfterBuilding_CreatedFileMustExists()
    {
        //Arrange
        using var systemUnderTests = FileSystem.BeginBuilding()
            .AddRandomInTempRootName()
            .AddFile("test-file.txt", out var filePath)
            .Build();
        //Act
        var result = File.Exists(filePath);
        //Assert
        result.Should().BeTrue();
    }
    
    [Theory]
    [AutoData]
    public void AddFileWithContent_AfterBuilding_CreatedFileMustHaveContent(string content)
    {
        //Arrange
        using var systemUnderTests = FileSystem.BeginBuilding()
            .AddRandomInTempRootName()
            .AddFile("test-file.txt", content, out var filePath)
            .Build();
        //Act
        var result = File.ReadAllText(filePath);
        //Assert
        result.Should().Be(content);
    }
}