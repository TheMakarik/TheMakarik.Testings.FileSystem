using System;
using System.Collections.Generic;
using System.IO;

namespace Arrange.FileSystem;

public class FileSystemArrangerContext
{
    public DirectoryInfo Directory { get; }

    public FileSystemArrangerContext(DirectoryInfo directory)
    {
        Directory = directory;
    }
}

public class FileSystemBuilder : IDisposable
{
    internal readonly IList<FileSystemArrangerEntry> _entries;
    private readonly bool _isTemp;

    public DirectoryInfo Directory { get; }

    public string FullName => Directory.FullName;

    public FileSystemBuilder(DirectoryInfo directory, bool isTemp)
    {
        _entries = new List<FileSystemArrangerEntry>();
        Directory = directory;
        _isTemp = isTemp;
    }

    public FileSystem Build()
    {
        FileSystemArrangerContext ctx = new FileSystemArrangerContext(Directory);
        foreach (FileSystemArrangerEntry entry in _entries)
            entry.Arrange(ctx);

        return new FileSystem(Directory, _isTemp);
    }

    public FileSystemBuilder AddEntry(FileSystemArrangerEntry entry)
    {
        _entries.Add(entry);
        return this;
    }

    public void Dispose()
    {
        foreach (FileSystemArrangerEntry entry in _entries)
        {
            if (entry is IDisposable disposable)
                disposable.Dispose();
        }

        _entries.Clear();
        GC.SuppressFinalize(this);
    }
}

public static class FileSystemBuilderExtensions
{
    public static FileSystemBuilder AddFile(this FileSystemBuilder builder, string filename, string content = "")
    {
        return builder.AddEntry(new FileArrangerEntry(filename, content));
    }

    public static FileSystemBuilder AddFile(this FileSystemBuilder builder, string filename, out string fullPath, string content = "")
    {
        fullPath = Path.Combine(builder.FullName, filename);
        return builder.AddEntry(new FileArrangerEntry(filename, content));
    }

    public static FileSystemBuilder AddFiles(this FileSystemBuilder builder, IEnumerable<string> filenames)
    {
        foreach (var filename in filenames)
        {
            builder.AddEntry(new FileArrangerEntry(filename, ""));
        }

        return builder;
    }

    public static FileSystemBuilder AddDirectory(this FileSystemBuilder builder, string name, Action<FileSystemBuilder> action)
    {
        DirectoryInfo directory = builder.Directory.CreateSubdirectory(name);
        FileSystemBuilder subBuilder = new FileSystemBuilder(directory, false);
        action(subBuilder);

        DirectoryArrangerEntry dirEntry = new DirectoryArrangerEntry(name, subBuilder._entries);
        return builder.AddEntry(dirEntry);
    }

    public static FileSystemBuilder AddDirectory(this FileSystemBuilder builder, string name, out string fullPath, Action<FileSystemBuilder> action)
    {
        DirectoryInfo directory = builder.Directory.CreateSubdirectory(name);
        FileSystemBuilder subBuilder = new FileSystemBuilder(directory, false);
        action(subBuilder);

        fullPath = directory.FullName;
        DirectoryArrangerEntry dirEntry = new DirectoryArrangerEntry(name, subBuilder._entries);
        return builder.AddEntry(dirEntry);
    }
}
