using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Arrange.FileSystem.ZipArranger;

public class ZipArrangerContext
{
    public ZipArchive Archive { get; }
    public DirectoryInfo Directory { get; }
    public string RelativePath { get; }
    public string Filename { get; }
    public CompressionLevel CompressionLevel { get; }

    public ZipArrangerContext(ZipArchive archive, DirectoryInfo directory, string relativePath, string filename, CompressionLevel compressionLevel)
    {
        Archive = archive;
        Directory = directory;
        RelativePath = relativePath;
        Filename = filename;
        CompressionLevel = compressionLevel;
    }

    public ZipArrangerContext(ZipArrangerContext other, string relativePath)
    {
        Archive = other.Archive;
        Directory = other.Directory;
        RelativePath = relativePath;
        Filename = other.Filename;
        CompressionLevel = other.CompressionLevel;
    }
}

public class ZipArchiveBuilder : IDisposable
{
    internal readonly IList<ZipArrangerEntry> _entries;

    public DirectoryInfo Directory { get; }
    public string Filename { get; }
    public string RelativePath { get; }
    public CompressionLevel CompressionLevel { get; }

    public string FullName
    {
        get => Path.Combine(Directory.FullName, Filename, RelativePath);
    }

    public ZipArchiveBuilder(DirectoryInfo directory, string filename, string relativePath, CompressionLevel compressionLevel)
    {
        _entries = [];
        
        Directory = directory;
        Filename = filename;
        RelativePath = relativePath;
        CompressionLevel = compressionLevel;
    }

    public ZipArchiveBuilder AddEntry(ZipArrangerEntry entry)
    {
        _entries.Add(entry);
        return this;
    }

    public void Dispose()
    {
        foreach (ZipArrangerEntry entry in _entries)
        {
            if (entry is IDisposable disposable)
                disposable.Dispose();
        }

        _entries.Clear();
        GC.SuppressFinalize(this);
    }
}

public static class ZipArchiveBuilderExtension
{
    public static ZipArchiveBuilder AddFile(this ZipArchiveBuilder builder, string filename, string content = "")
    {
        return builder.AddEntry(new ZipFileArrangerEntry(filename, content));
    }

    public static ZipArchiveBuilder AddFile(this ZipArchiveBuilder builder, string filename, out string fullPath, string content = "")
    {
        fullPath = Path.Combine(builder.FullName, filename);
        return builder.AddEntry(new ZipFileArrangerEntry(filename, content));
    }

    public static ZipArchiveBuilder AddFiles(this ZipArchiveBuilder builder, IEnumerable<string> filenames)
    {
        foreach (var filename in filenames)
        {
            builder.AddEntry(new ZipFileArrangerEntry(filename, ""));
        }

        return builder;
    }

    public static ZipArchiveBuilder AddDirectory(this ZipArchiveBuilder builder, string name, Action<ZipArchiveBuilder> action)
    {
        string relativePath = Path.Combine(builder.RelativePath, name);
        ZipArchiveBuilder subBuilder = new ZipArchiveBuilder(builder.Directory, builder.Filename, relativePath, builder.CompressionLevel);
        action(subBuilder);

        ZipDirectoryArrangerEntry dirEntry = new ZipDirectoryArrangerEntry(name, subBuilder._entries);
        return builder.AddEntry(dirEntry);
    }

    public static ZipArchiveBuilder AddDirectory(this ZipArchiveBuilder builder, string name, out string fullPath, Action<ZipArchiveBuilder> action)
    {
        string relativePath = Path.Combine(builder.RelativePath, name);
        ZipArchiveBuilder subBuilder = new ZipArchiveBuilder(builder.Directory, builder.Filename, relativePath, builder.CompressionLevel);
        action(subBuilder);

        fullPath = Path.Combine(builder.Directory.FullName, builder.Filename, relativePath);
        ZipDirectoryArrangerEntry dirEntry = new ZipDirectoryArrangerEntry(name, subBuilder._entries);
        return builder.AddEntry(dirEntry);
    }
}
