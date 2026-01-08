using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheMakarik.Testing.FileSystem.Assertion;
using TheMakarik.Testing.FileSystem.Core;

namespace TheMakarik.Testing.FileSystem;

public sealed class FileSystem : IFileSystem
{
    
    #region Static IFileSystemBuilder constructor

    /// <summary>
    /// Static <see cref="IFileSystemBuilder"/> constructor
    /// </summary>
    /// <returns>A new instance of <see cref="IFileSystemBuilder"/></returns>
    public static IFileSystemBuilder BeginBuilding()
    {
        return new FileSystemBuilder();
    }

    #endregion
    
    #region Construtors

    internal FileSystem(string root)
    {
        this.Root = root;
    }
    
    #endregion
   
    #region IFileSystem implementation
    public string Root { get; private set; }

    public IFileSystem In(string relativePath)
    {
        return new FileSystem(Path.Combine(this.Root, relativePath));
    }

    public IFileSystemAssertion Should()
    {
        return new FileSystemAssertion(this);
    }
    
    #endregion
    
    #region IEnumerable implementation
    
    
    public IEnumerator<string> GetEnumerator()
    {
        return EnumerateRootContent().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
    
    #endregion
  

    #region IDisposable implementation

    public void Dispose()
    {
        Directory.Delete(this.Root, recursive: true);
    }

    #endregion
    
    #region Private methods
    
    private IEnumerable<string> EnumerateRootContent()
    {
        return Directory
            .EnumerateDirectories(this.Root)
            .Concat(Directory.EnumerateFiles(this.Root)
            ).ToArray();
    }
    
    #endregion
    
}