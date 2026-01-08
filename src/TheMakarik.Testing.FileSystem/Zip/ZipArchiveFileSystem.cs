using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TheMakarik.Testing.FileSystem.Assertion;

namespace TheMakarik.Testing.FileSystem.Zip;

public class ZipArchiveFileSystem : IFileSystem
{
    #region Properties

    public string Root { get; }
    
    #endregion

    #region Constructor

    internal ZipArchiveFileSystem(string root)
    {
        Root = root;
    }

    #endregion
    
    public IEnumerator<string> GetEnumerator()
    {
        throw new System.NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

   
    public IFileSystem In(string relativePath)
    {
        throw new System.NotImplementedException();
    }

    public IFileSystemAssertion Should()
    {
        throw new System.NotImplementedException();
    }
    
    #region Dispose pattern

    public void Dispose()
    {
        File.Delete(Root);
    }

    #endregion
    
    #region Private methods
    
    private IZipArchiveBuilder GetContent()
    {
        return null;
    }
    
    #endregion
}