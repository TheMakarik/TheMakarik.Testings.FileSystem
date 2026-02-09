using TheMakarik.Testing.FileSystem.AutoNaming;

namespace TheMakarik.Testing.FileSystem.Tests;

public class NameGeneratorTests : IDisposable
{
    private const int TextFilesCount = 100;
    private readonly IFileSystem _systemUnderTests;
    private string _directoryPath;

    public NameGeneratorTests()
    {
        _systemUnderTests = FileSystem
            .BeginBuilding()
            .AddRandomInTempRootName()
            .AddNameGenerator(NameGenerationType.RandomNameAndCount)
            .AddDirectoryWithNameGenerating(out _directoryPath, (_, builder) =>
            {
                for (var i = 0; i < TextFilesCount; i++)
                {
                    builder.AddFileWithNameGeneraing(".txt", "HelloWorld");
                }

                return builder;
            })
            .Build();
    }

    [Fact]
    public void FileSystem_MustHaveTextFilesCountFiles()
    {
        _systemUnderTests.Should().DirectoryHasFileCount(_directoryPath,   TextFilesCount);
    }

    public void Dispose()
    {
        _systemUnderTests.Dispose();
    }
}