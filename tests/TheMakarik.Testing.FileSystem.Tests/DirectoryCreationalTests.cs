using AutoFixture.Xunit2;
using FluentAssertions;

namespace TheMakarik.Testing.FileSystem.Tests;

public class DirectoryCreationalTests
{
    [Fact]
    public void AddDirectory_AfterBuilding_CreatedDirectoryMustExists()
    {
        //Arrange
        using var systemUnderTests = FileSystem.BeginBuilding()
            .AddRandomInTempRootName()
            .AddDirectory("directory", out var directory)
            .Build();
        //Act
        var result = Directory.Exists(directory);
        //Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void AddDirectory_AfterBuildingWithSubDirectory_CreatedSubDirectoryMustExists()
    {
        //Arrange
        var subDirectory = string.Empty;
        using var systemUnderTests = FileSystem.BeginBuilding()
            .AddRandomInTempRootName()
            .AddDirectory("directory", out var directory, (path, builder) =>
                builder.AddDirectory("subDirectory", out subDirectory))
            .Build();
        //Act
        var result = Directory.Exists(subDirectory);
        //Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void AddDirectory_AfterBuildingWithFile_CreatedFileMustExists()
    {
        //Arrange
        var filePath = string.Empty;
        using var systemUnderTests = FileSystem.BeginBuilding()
            .AddRandomInTempRootName()
            .AddDirectory("directory", out var directory, (path, builder) =>
                builder.AddFile("file", out filePath))
            .Build();
        //Act
        var result = File.Exists(filePath);
        //Assert
        result.Should().BeTrue();
    }
}