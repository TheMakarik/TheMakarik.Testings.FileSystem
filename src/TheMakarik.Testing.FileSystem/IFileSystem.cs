using System;
using System.Collections.Generic;
using TheMakarik.Testing.FileSystem.Assertion;

namespace TheMakarik.Testing.FileSystem;

/// <summary>
/// Represent default abstractions for FileSystem class
/// </summary>
public interface IFileSystem : IDisposable, IEnumerable<string>
{
    /// <summary>
    /// Root file system folder's path where all <see cref="IFileSystem"/> content contains
    /// </summary>
    public string RootPath { get; }

    /// <summary>
    /// Gets the <see cref="IFileSystem"/> of deeper directory by relative path
    /// </summary>
    /// <param name="relativePath">Root-relatove directory path</param>
    /// <returns><see cref="IFileSystem"/> of <see cref="relativePath"/></returns>
    public IFileSystem In(string relativePath);
    
    /// <summary>
    /// Start <see cref="IFileSystem"/> assertion for integrational tests
    /// </summary>
    /// <returns>A new instance of <see cref="IFileSystemAssertion"/> for assertings </returns>
    public IFileSystemAssertion Should();
   
}