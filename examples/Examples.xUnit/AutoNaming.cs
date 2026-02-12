using TheMakarik.Testing.FileSystem;
using TheMakarik.Testing.FileSystem.AutoNaming;
using TheMakarik.Testing.FileSystem.Core;

namespace Examples.xUnit;

public class AutoNaming : IDisposable
{
    private readonly IFileSystem _fileSystem;

    public AutoNaming()
    {
        _fileSystem = FileSystem.BeginBuilding()
            .AddRandomInTempRootName()
            .AddNameGenerator(NameGenerationType.RandomNameAndCount)
            .AddFilesWithNameGenerating(".txt", 30, "Hello World!")
            .AddFilesWithNameGenerating(".json", 100)
            .AddDirectoryWithNameGenerating((path, builder) => builder
                .RefreshNameGenerator()
                .AddFileWithNameGeneraing(".db"))
            .Build();
    }

    [Fact]
    public void AllFilesWithTxtExtensions_MustContainsHelloWorld()
    {
        _fileSystem.Should().Be(path =>
        {
            if (Path.GetExtension(path) != ".txt")
                return true;
            return File.ReadAllText(path) ==  "Hello World!";

        });
    }


    public void Dispose()
    {
        _fileSystem.Dispose();
    }
}