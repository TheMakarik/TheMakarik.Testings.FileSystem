
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Arrange.FileSystem.ZipArranger;

public abstract class ZipArrangerEntry
{
    public abstract void Arrange(ZipArrangerContext context);
}

public class ZipFileArrangerEntry : ZipArrangerEntry
{
    private readonly string _filename;
    private readonly string _content;

    public ZipFileArrangerEntry(string filename, string content)
    {
        _filename = filename;
        _content = content;
    }

    public override void Arrange(ZipArrangerContext context)
    {
        ZipArchiveEntry entry = context.Archive.CreateEntry(Path.Combine(context.RelativePath, _filename), context.CompressionLevel);
        using Stream entryStream = entry.Open();

        byte[] buffer = Encoding.UTF8.GetBytes(_content);
        entryStream.Write(buffer, 0, buffer.Length);
    }
}

public class ZipDirectoryArrangerEntry : ZipArrangerEntry
{
    private readonly string _dirname;
    private readonly IList<ZipArrangerEntry> _entries;

    public ZipDirectoryArrangerEntry(string dirname, IList<ZipArrangerEntry> entries)
    {
        _dirname = dirname;
        _entries = entries;
    }

    public override void Arrange(ZipArrangerContext context)
    {
        string relativePath = string.IsNullOrEmpty(context.RelativePath) ? _dirname : context.RelativePath + "\\" + _dirname;
        ZipArrangerContext subCtx = new ZipArrangerContext(context, relativePath);
        foreach (ZipArrangerEntry entry in _entries)
            entry.Arrange(subCtx);
    }
}
