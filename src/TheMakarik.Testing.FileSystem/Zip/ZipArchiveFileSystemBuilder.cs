using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using TheMakarik.Testing.FileSystem.Objects;

namespace TheMakarik.Testing.FileSystem.Zip;

public sealed class ZipArchiveFileSystemBuilder(string root, string? prefix = null) : IZipArchiveFileSystemBuilder
{
    #region Fields
    
    private HashSet<Action<ZipArchive, string>> _builderActions = new(capacity: 10);

    #endregion
    #region IZipArchiveFileSystemBuilder implementation

    public string Root { get; } = root;
    public string Prefix { get; } = prefix ?? string.Empty;

    public IZipArchiveFileSystemBuilder Add(string relativePath, Action<ZipArchive, string> additionalAction)
    {
       _builderActions.Add(additionalAction);   
       return this;
    }

    public void Build()
    {
        Guard.AgainstNull(Root, nameof(root));
        
        Debug.Assert(Path.GetExtension(Root) == ".zip");
        using var zipStream = File.Create(Root);
        using var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create);
        try
        {
            foreach (var action in _builderActions)
                action(zipArchive, Root);
        }
        catch (Exception e)
        {
            if(File.Exists(Root))
                File.Delete(Root);
            throw;
        }
    }
    
    #endregion
  
}