using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TheMakarik.Testing.FileSystem.Arrangement;
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
   
    #region IFileSystem implementation
    
    public string RootPath { get; }
    
    public IFileSystemAssertion Should()
    {
        throw new NotImplementedException();
    }
    
    #endregion
    
    
    #region ICollection implementation
    
    public int Count { get; }
    public bool IsReadOnly => true;
    
    public IEnumerator<string> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(string item)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(string item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(string[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(string item)
    {
        throw new NotImplementedException();
    }

  

    #endregion
  

    #region IDisposable implementation

    public void Dispose()
    {
        // TODO release managed resources here
    }

    #endregion
    
}