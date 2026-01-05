using System.Collections.Generic;
using System.IO;

namespace Arrange.FileSystem;

public abstract class FileSystemArrangerEntry
{
    public abstract void Arrange(FileSystemArrangerContext context);
}

public class FileArrangerEntry : FileSystemArrangerEntry
{
    private readonly string _filename;
    private readonly string _content;

    public FileArrangerEntry(string filename, string content)
    {
        _filename = filename;
        _content = content;
    }

    public override void Arrange(FileSystemArrangerContext context)
    {
        File.WriteAllText(Path.Combine(context.Directory.FullName, _filename), _content);
    }
}

public class DirectoryArrangerEntry : FileSystemArrangerEntry
{
    private readonly string _dirname;
    private readonly IList<FileSystemArrangerEntry> _entries;

    public DirectoryArrangerEntry(string dirname, IList<FileSystemArrangerEntry> entries)
    {
        _dirname = dirname;
        _entries = entries;
    }

    public override void Arrange(FileSystemArrangerContext context)
    {
        DirectoryInfo subDir = context.Directory.CreateSubdirectory(_dirname);
        FileSystemArrangerContext subCtx = new FileSystemArrangerContext(subDir);

        foreach (FileSystemArrangerEntry entry in _entries)
            entry.Arrange(subCtx);
    }
}
