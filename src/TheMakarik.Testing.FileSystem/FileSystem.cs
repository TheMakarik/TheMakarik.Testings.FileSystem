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
/// Encapsulates a temporary directory as a file system and enables assertions on its contents.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="FileSystem"/> class represents a temporary file system that can be used for testing purposes.
/// It provides methods for navigating the file system hierarchy, validating file system state through assertions,
/// and automatically cleaning up temporary resources when disposed.
/// </para>
/// <para>
/// All file system operations are performed relative to the <see cref="Root"/> directory.
/// The file system can be created either from an existing directory or built from scratch using the <see cref="BeginBuilding"/> method.
/// </para>
/// </remarks>
[Serializable]
[DebuggerDisplay("{Root} with content {DebuggerDisplayContent}")]
public sealed class FileSystem : IFileSystem
{
    #region Consts

    private const int DebuggerMaxFileSystemEntriesCount = 6;
    
    #endregion
    
    #region Static IFileSystemBuilder constructor
    
    /// <summary>
    /// Creates a new instance of <see cref="IFileSystemBuilder"/> for constructing file systems.
    /// </summary>
    /// <returns>A new instance of <see cref="IFileSystemBuilder"/> that can be used to build a file system.</returns>
    /// <remarks>
    /// Use this method when you need to create a file system from a non-existent directory.
    /// The builder provides a fluent interface for creating complex file system structures.
    /// </remarks>
    public static IFileSystemBuilder BeginBuilding()
    {
        return new FileSystemBuilder();
    }

    #endregion

    #region Event handlers

    /// <inheritdoc/>
    public event EventHandler? Disposed;
    
    /// <inheritdoc/>
    public event EventHandler? AssertionStart;

    #endregion
    
    #region Constructors

    /// <summary>
    /// Creates an instance of <see cref="FileSystem"/> from an existing directory.
    /// </summary>
    /// <param name="root">The path to an existing directory that will serve as the root of the file system.</param>
    /// <remarks>
    /// <para>
    /// This constructor creates a file system wrapper around an existing directory.
    /// The directory must already exist at the specified path.
    /// </para>
    /// <para>
    /// For creating file systems from non-existent directories, use <see cref="BeginBuilding"/> instead.
    /// </para>
    /// </remarks>
    /// <exception cref="System.IO.DirectoryNotFoundException">
    /// Thrown when the specified directory does not exist.
    /// </exception>
    public FileSystem(string root)
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
        AssertionStart?.Invoke(this, EventArgs.Empty);
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
    /// Recursively deletes the root directory and all its contents.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method performs a recursive deletion of the entire file system structure.
    /// After disposal, the <see cref="Disposed"/> event is raised, and the file system
    /// should no longer be used.
    /// </para>
    /// <para>
    /// It is recommended to use this class within a <c>using</c> statement to ensure
    /// proper cleanup of temporary resources.
    /// </para>
    /// </remarks>
    /// <exception cref="System.IO.IOException">
    /// Thrown when files or directories cannot be deleted (e.g., due to permissions or file locks).
    /// </exception>
    public void Dispose()
    {
        Disposed?.Invoke(this, EventArgs.Empty);
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
            
            if (content.Length > DebuggerMaxFileSystemEntriesCount)
                display += $", ... (+{content.Length - DebuggerMaxFileSystemEntriesCount} more)";
                
            return display;
        }
    }
    
    private string[] GetRootContent()
    {
        return EnumerateRootContent().ToArray();
    }
    
    #endregion
}