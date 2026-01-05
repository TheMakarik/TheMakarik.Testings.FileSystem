using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Arrange.FileSystem.ZipArranger;

public class ZipArchiveArrangerEntry : FileSystemArrangerEntry
{
    private readonly IList<ZipArrangerEntry> _entries;
    private readonly string _filename;
    private readonly CompressionLevel _compressionLevel;

    public ZipArchiveArrangerEntry(string filename, CompressionLevel compressionLevel, IList<ZipArrangerEntry> entries)
    {
        _entries = entries;
        _filename = filename;
        _compressionLevel = compressionLevel;
    }

    public sealed override void Arrange(FileSystemArrangerContext context)
    {
        string filename = Path.Combine(context.Directory.FullName, _filename);
        using FileStream stream = new FileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite);
        using ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create);

        ZipArrangerContext ctx = new ZipArrangerContext(archive, context.Directory, "", _filename, _compressionLevel);
        foreach (ZipArrangerEntry entry in _entries)
            entry.Arrange(ctx);

        stream.Flush();
    }
}

public static class FileSystemBuilderExtension
{
    public static FileSystemBuilder AddZipArchive(this FileSystemBuilder builder, string filename, Action<ZipArchiveBuilder> action)
    {
        CompressionLevel compressionLevel = CompressionLevel.NoCompression;

        ZipArchiveBuilder zipBuilder = new ZipArchiveBuilder(builder.Directory, filename, "", compressionLevel);
        action(zipBuilder);

        ZipArchiveArrangerEntry zipEntry = new ZipArchiveArrangerEntry(filename, compressionLevel, zipBuilder._entries);
        return builder.AddEntry(zipEntry);
    }

    public static FileSystemBuilder AddZipArchive(this FileSystemBuilder builder, string filename, out string fullPath, Action<ZipArchiveBuilder> action)
    {
        CompressionLevel compressionLevel = CompressionLevel.NoCompression;

        ZipArchiveBuilder zipBuilder = new ZipArchiveBuilder(builder.Directory, filename, "", compressionLevel);
        action(zipBuilder);

        fullPath = Path.Combine(builder.Directory.FullName, filename);
        ZipArchiveArrangerEntry zipEntry = new ZipArchiveArrangerEntry(filename, compressionLevel, zipBuilder._entries);
        return builder.AddEntry(zipEntry);
    }

    public static FileSystemBuilder AddZipArchive(this FileSystemBuilder builder, string filename, CompressionLevel compressionLevel, Action<ZipArchiveBuilder> action)
    {
        ZipArchiveBuilder zipBuilder = new ZipArchiveBuilder(builder.Directory, filename, "", compressionLevel);
        action(zipBuilder);

        ZipArchiveArrangerEntry zipEntry = new ZipArchiveArrangerEntry(filename, compressionLevel, zipBuilder._entries);
        return builder.AddEntry(zipEntry);
    }

    public static FileSystemBuilder AddZipArchive(this FileSystemBuilder builder, string filename, CompressionLevel compressionLevel, out string fullPath, Action<ZipArchiveBuilder> action)
    {
        ZipArchiveBuilder zipBuilder = new ZipArchiveBuilder(builder.Directory, filename, "", compressionLevel);
        action(zipBuilder);

        fullPath = Path.Combine(builder.Directory.FullName, filename);
        ZipArchiveArrangerEntry zipEntry = new ZipArchiveArrangerEntry(filename, compressionLevel, zipBuilder._entries);
        return builder.AddEntry(zipEntry);
    }
}
