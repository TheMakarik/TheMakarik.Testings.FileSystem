using TheMakarik.Testing.FileSystem;

namespace Examples.xUnit;

public class FileCreational : IDisposable
{
    private readonly IFileSystem _fileSystem = FileSystem.BeginBuilding()
        .AddInTempRoot("MY_TEST_TEMP_FOLDER")
        .AddFile("my-test-file.txt")
        .AddFile("my-test-file2.txt", "has content")
        .AddFile("my-test-file3.txt", "has content", out var fullPath)
        .Build();


    public void Dispose()
    {
        _fileSystem.Dispose();
    }
}