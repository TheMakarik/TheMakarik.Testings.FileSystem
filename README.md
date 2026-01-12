<div align="center">
  <h1>
    <img src="img/icon.svg" alt="TheMakarik Icon" width="48" height="48" style="vertical-align: middle;">
    <span style="margin-left: 12px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text;">
      TheMakarik.Testing.FileSystem
    </span>
  </h1>

  <p style="font-size: 1.2em; color: #666; margin-top: -10px; margin-bottom: 30px;">
    🧪 Fluent API for directory assertions and creations for your integrational tests in .NET
  </p>

  
</div>


## Quick start (<a href="https://xunit.net/?tabs=cs">xUnit.net</a>)
#### 1. Create new project
```shell
dotnet new xunit --language C#
```
#### 2. Create a simple tests class
```csharp
using System.Collections.Generic;
using System.IO;

namespace ReadMeExample

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
```

#### 3. Create some tests
```csharp
using TheMakarik.Testing.FileSystem;
using TheMakarik.Testing.FileSystem.Zip;
using ReadMeExample;

namespace ReadMeExample.Tests;

public class ReadMeExampleTestts
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
```


## Problem
The problem that the package resolve it's testing Explorer state machine between archives and directories at [SolidZip](https://github.com/TheMakarik/solid-zip), I guess this library will be useful for someone else

## Documentation
Documentation is available in two languages:
- **Russian:** [here](./docs/ru)
- **English:** [here](./docs/eng)