using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Arrange.FileSystem;

public class FileSystem : IDisposable, IEnumerable<FileSystemInfo>
{
    private readonly bool _isTemp;

    public DirectoryInfo Directory { get; }

    internal FileSystem(DirectoryInfo directory, bool isTemp)
    {
        Directory = directory;
        _isTemp = isTemp;
    }

    public static FileSystemBuilder Begin(string? name = null)
    {
        string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        if (!string.IsNullOrEmpty(name))
            tempDir = Path.Combine(tempDir, name);

        DirectoryInfo directory = new DirectoryInfo(tempDir);
        directory.Create();

        return new FileSystemBuilder(directory, true);
    }

    public void Destroy()
    {
        if (_isTemp && Directory.Exists)
        {
            Directory.Delete(true);
        }
    }

    public void Dispose()
    {
        Destroy();
        GC.SuppressFinalize(this);
    }

    public IEnumerator<FileSystemInfo> GetEnumerator() => Directory.EnumerateFileSystemInfos().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
