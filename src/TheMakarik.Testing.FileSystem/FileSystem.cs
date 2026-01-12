using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TheMakarik.Testing.FileSystem.Assertion;
using TheMakarik.Testing.FileSystem.Core;

namespace TheMakarik.Testing.FileSystem;

/// <summary>
/// Encapsulate the temporary directory like a file system, and enable to assert it
/// </summary>
[Serializable]
[DebuggerDisplay("{Root} with content {DebuggerDisplayContent}")]
public sealed class FileSystem : IFileSystem
{
    #region Consts

    private const int DebuggerMaxFileSystemEntriesCount = 6;
    
    #endregion
    
    
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
    
    /// <inheritdoc/>
    public string Root { get; private set; }

    /// <inheritdoc/>
    public IFileSystem In(string relativePath)
    {
        return new FileSystem(Path.Combine(this.Root, relativePath));
    }

    /// <inheritdoc/>
    public IFileSystemAssertion Should()
    {
        return new FileSystemAssertion(this);
    }
    
    #endregion
    
    #region IEnumerable implementation
    
    /// <inheritdoc/>
    public IEnumerator<string> GetEnumerator()
    {
        return EnumerateRootContent().GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
    
    #endregion
  

    #region IDisposable implementation

    /// <summary>
    /// Deleted the directory recursive
    /// </summary>
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
            );
    }
    
    private string DebuggerDisplayContent
    {
        get
        {
            var content = GetRootContent();
            if (content.Length == 0) return "Empty";

            var items = content.Take(DebuggerMaxFileSystemEntriesCount);
            var display = string.Join(", ", items);
            
            if (content.Length >DebuggerMaxFileSystemEntriesCount )
                display += $", ... (+{content.Length - DebuggerMaxFileSystemEntriesCount } more)";
                
            return display;
        }
    }
    
    private string[] GetRootContent()
    {
        return EnumerateRootContent().ToArray();
    }
    
    #endregion
    
}