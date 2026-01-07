using TheMakarik.Testing.FileSystem;

namespace Examples.xUnit;

public class AddingDirectoryWithContent : IDisposable
{
    private readonly IFileSystem _fileSystem = FileSystem.BeginBuilding()
        .AddInTempRoot("MY_TEST_TEMP_FOLDER")
        .AddDirectory("test-folder", (_, builder) => builder
            .AddFile("test-file.txt")
            .AddDirectory("test-subdir", (_, builder) => builder
                .AddFile("hello.txt")
                .AddDirectory("directory-without-content"))
            .AddFile("test-file2.txt"))  
        .Build();
    
    public void Dispose()
    {
        _fileSystem.Dispose();
    }
}