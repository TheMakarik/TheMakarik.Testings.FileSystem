using System;
using System.Collections.Generic;
using System.IO.Compression;
using TheMakarik.Testing.FileSystem.Objects;

namespace TheMakarik.Testing.FileSystem.Zip;

public class ZipArchiveBuilder : IZipArchiveBuilder
{
    #region Fields

    private string? _root;
    private HashSet<Action<ZipArchive, IZipArchiveBuilder>> _builderActions = new(capacity: 10);

    #endregion
    #region IZipArchiveBuilder implementation
    
    public IZipArchiveBuilder Add(string relativePath, Action<ZipArchive, IZipArchiveBuilder> additionalAction)
    {
       _builderActions.Add(additionalAction);   
       return this;
    }

    public IZipArchiveFileSystem Build()
    {
        throw new NotImplementedException();
    }
    
    #endregion
  
}