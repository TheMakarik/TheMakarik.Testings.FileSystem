using FluentAssertions;
using SharpCompress.Archives.GZip;
using SharpCompress.Archives.Tar;
using SharpCompress.Compressors.BZip2;
using TheMakarik.Testing.FileSystem.SharpCompress.Tar;

namespace TheMakarik.Testing.FileSystem.Tests;

public class TarFileSystemBuilderTests
{
    [Theory]
    [InlineData("my-tar", TarPackTo.BZip2, ".bz2")]
    [InlineData("my-tar", TarPackTo.GZip, ".gz")]
    [InlineData("my-tar", TarPackTo.None, ".tar")]
    public void TarArchive_MustNormalizeExtensions(string name, TarPackTo packTo, string exptectedExtension)
    {
        //Arrange
        using var systemUnderTests = FileSystem.BeginBuilding()
            .AddRandomInTempRootName()
            .AddTar(name, out var fullName, builder => builder, packTo)
            .Build();
        //Act
        var extension = Path.GetExtension(fullName);
        //Assert
        extension.Should().Be(exptectedExtension);
        
    }

    [Fact]
    public void TarGzArchive_MustBeRecognizedAsGzip()
    {
        //Arrange
        using var systemUnderTests = FileSystem.BeginBuilding()
            .AddRandomInTempRootName()
            .AddTar("test",  out var name, builder => builder, TarPackTo.GZip)
            .Build();
        //Act
        var result = GZipArchive.IsGZipFile(name);
        //Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void TarBz2Archive_MustBeRecognizedAsBzip2()
    {
        //Arrange
        using var systemUnderTests = FileSystem.BeginBuilding()
            .AddRandomInTempRootName()
            .AddTar("test", out var name, builder => builder, TarPackTo.BZip2)
            .Build();
        using var stream = File.OpenRead(Path.Combine(name));
        //Act
        var result = BZip2Stream.IsBZip2(stream);
        //Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void TarArchive_MustBeRecognizedAsTar()
    {
        //Arrange
        using var systemUnderTests = FileSystem.BeginBuilding()
            .AddRandomInTempRootName()
            .AddTar("test",  out var name, builder => builder, TarPackTo.None)
            .Build();
        
        //Act
        var result = TarArchive.IsTarFile(name);
        //Assert
        result.Should().BeTrue();
    }
}