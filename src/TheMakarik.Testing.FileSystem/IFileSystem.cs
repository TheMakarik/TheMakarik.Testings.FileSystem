using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TheMakarik.Testing.FileSystem.Assertion;

namespace TheMakarik.Testing.FileSystem;

/// <summary>
/// Represent default abstractions for FileSystem class
/// </summary>
[PublicAPI]
public interface IFileSystem : IDisposable, IEnumerable<string>
{
    /// <summary>
    /// Event that occurs then the <see cref="IFileSystem"/> instance starts disposing
    /// </summary>
    public event EventHandler Disposed;

    /// <summary>
    /// Events that occurs then the <see cref="IFileSystem"/> instance starts an assertion
    /// </summary>
    public event EventHandler AssertionStart;
    
    /// <summary>
    /// Root file system folder's path where all <see cref="IFileSystem"/> content contains
    /// </summary>
    public string Root { get; }

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