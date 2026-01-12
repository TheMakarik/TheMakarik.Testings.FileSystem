using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using TheMakarik.Testing.FileSystem.Assertion;

namespace TheMakarik.Testing.FileSystem.Zip;

[DebuggerDisplay("{Root} with content {DebuggerDisplayContent}")]
public sealed class ZipArchiveFileSystem : IZipArchiveFileSystem
{
    public ZipArchiveFileSystem(string root)
    {
        Root = root;
        ZipEntry = ZipFile.OpenRead(Root);
    }

    #region IZipArchiveFileSystem
    
    public string Root { get; }
    public ZipArchive ZipEntry { get; }

    public IZipArchiveAssertion Should()
    {
        throw new NotImplementedException();
    }
    
    #endregion
    
    
    #region IEnumerable
    
    public IEnumerator<string> GetEnumerator()
    {
        return this.EnumerateZipContent().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    #endregion

    #region IDisposable

    public void Dispose()
    { 
        this.ZipEntry.Dispose();
        File.Delete(Root);
    }

    #endregion
    
    #region Private methods

    private IEnumerable<string> EnumerateZipContent()
    {
        return this.ZipEntry.Entries.Select(e => e.FullName);
    }
    
    public string[] DebuggerDisplayContent => EnumerateZipContent().ToArray();
    

    
    #endregion
}