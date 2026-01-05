using System;
using System.Collections.Generic;
using TheMakarik.Testing.FileSystem.Arrangement;

namespace TheMakarik.Testing.FileSystem;

/// <summary>
/// Represent default abstractions for FileSystem class
/// </summary>
public interface IFileSystem : IDisposable, IEnumerable<string>
{
   
    /// <summary>
    /// Represent the count of the elements at the file system
    /// </summary>
    public int Count { get; }
    
    /// <summary>
    /// Root file system folder's path where all <see cref="IFileSystem"/> content contains
    /// </summary>
    public string RootPath { get; }
    
    /// <summary>
    /// Start <see cref="IFileSystem"/> assertion for integrational tests
    /// </summary>
    /// <returns>A new instance of <see cref="IFileSystemAssertion"/> for assertings </returns>
    public IFileSystemAssertion Should();
   
}