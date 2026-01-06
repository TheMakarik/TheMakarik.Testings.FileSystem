using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TheMakarik.Testing.FileSystem.Objects;

namespace TheMakarik.Testing.FileSystem.Core;

/// <summary>
/// Builder for creating temporary file system structures for testing purposes.
/// Implements the <see cref="IFileSystemBuilder"/> interface to provide fluent API for file system construction.
/// </summary>
/// <remarks>
/// This class is responsible for building a hierarchical file system structure in a temporary location.
/// It tracks all created files and directories and ensures proper cleanup on disposal.
/// </remarks>
public sealed class FileSystemBuilder : IFileSystemBuilder
{
    #region Fields

    private string? _root;
    private HashSet<FileSystemCreationalContent> _builderActions = new(capacity: 10);
    
    #endregion
    
    #region Properties
    
    /// <summary>
    /// Gets the root directory path for the file system being built.
    /// </summary>
    /// <value>The full path to the root directory.</value>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the root directory has not been set. Call <see cref="AddRoot"/> first.
    /// </exception>
    public string RootDirectory => _root ?? throw
        new InvalidOperationException("Cannot get the root directory because it is not declared, use AddRoot(path) method to declare");

    #endregion

    #region IFileSystemBuilder implementation
    
    /// <summary>
    /// Sets the root directory for the file system being built.
    /// </summary>
    /// <param name="root">The full path to the root directory.</param>
    /// <returns>The current <see cref="FileSystemBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="root"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when attempting to set the root directory more than once.</exception>
    /// <remarks>
    /// This method must be called before any other file system operations.
    /// The root directory will be created when <see cref="Build"/> is called.
    /// </remarks>
    public IFileSystemBuilder AddRoot(string root)
    {
        Guard.AgainstNull(root);
        
        if (this._root is not null)
            throw new InvalidOperationException("Cannot add root directory twice");
        this._root = root;
        
        return this;
    }

    /// <summary>
    /// Adds a file system element (file, directory or something else) at the specified relative path.
    /// </summary>
    /// <param name="rootRelativePath">The relative path from the root directory.</param>
    /// <param name="additionalAction">
    /// The action that creates the file system element. 
    /// Receives the full path where the element should be created and the current builder instance.
    /// </param>
    /// <returns>The current <see cref="FileSystemBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rootRelativePath"/> is null.</exception>
    /// <remarks>
    /// The relative path is added to the <see cref="Content"/> collection.
    /// The actual creation of the file system element is deferred until <see cref="Build"/> is called.
    /// </remarks>
    public IFileSystemBuilder Add(string rootRelativePath, Action<string, IFileSystemBuilder> additionalAction)
    {
        Guard.AgainstNull(rootRelativePath);
        this._builderActions.Add(new FileSystemCreationalContent(additionalAction, rootRelativePath));
        return this;
    }

    /// <summary>
    /// Builds the file system structure by executing all queued creation actions.
    /// </summary>
    /// <returns>An <see cref="IFileSystem"/> instance representing the created file system.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the root directory is not set.</exception>
    /// <exception cref="IOException">Thrown when file system operations fail.</exception>
    /// <remarks>
    /// <para>
    /// This method performs the following steps:
    /// 1. Creates the root directory if it doesn't exist.
    /// 2. Executes all queued creation actions in the order they were added.
    /// 3. If any exception occurs, cleans up the entire file structure and rethrows the exception.
    /// 4. Returns a <see cref="FileSystem"/> instance that tracks the created structure.
    /// </para>
    /// <para>
    /// The cleanup on failure ensures that no partial file structures are left behind.
    /// </para>
    /// </remarks>
    public IFileSystem Build()
    {
        Directory.CreateDirectory(this.RootDirectory);
        try
        {
            foreach (var action in this._builderActions)
                action.InvokeBuildingAction(this);
        }
        catch (Exception e)
        {
            Directory.Delete(this.RootDirectory, recursive: true);
            throw;
        }
       
        return new FileSystem(this.RootDirectory);
    }
    
    #endregion
}