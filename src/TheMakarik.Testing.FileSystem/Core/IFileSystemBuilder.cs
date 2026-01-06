using System;
using System.Collections.Generic;

namespace TheMakarik.Testing.FileSystem.Core;

/// <summary>
/// Defines a fluent interface for building temporary file system structures for unit testing.
/// </summary>
/// <remarks>
/// This interface provides methods to declaratively construct file and directory hierarchies
/// that can be used in isolated test environments. Implementations typically create structures
/// in temporary locations and ensure proper cleanup.
/// </remarks>
public interface IFileSystemBuilder
{
    /// <summary>
    /// Gets the root directory path for the file system being built.
    /// </summary>
    /// <value>
    /// The full path to the root directory where all file system elements will be created.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// Thrown when accessed before <see cref="AddRoot"/> has been called.
    /// </exception>
    string RootDirectory { get; }

  

    /// <summary>
    /// Sets the root directory for the file system being built.
    /// </summary>
    /// <param name="root">The full path to the root directory.</param>
    /// <returns>The current <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="root"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when attempting to set the root directory more than once.</exception>
    /// <remarks>
    /// This method must be called before adding any files or directories.
    /// The root directory will be physically created when <see cref="Build"/> is called.
    /// </remarks>
    IFileSystemBuilder AddRoot(string root);

    /// <summary>
    /// Adds a file system element (file or directory) at the specified relative path.
    /// </summary>
    /// <param name="rootRelativePath">
    /// The relative path from the root directory where the element should be created.
    /// Can include subdirectories (e.g., "folder/file.txt").
    /// </param>
    /// <param name="additionalAction">
    /// An action that performs the actual creation of the file system element.
    /// The action receives:
    /// <list type="bullet">
    /// <item><description>The full path where the element should be created</description></item>
    /// <item><description>The current builder instance for creating nested structures</description></item>
    /// </list>
    /// </param>
    /// <returns>The current <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rootRelativePath"/> is null.</exception>
    /// <remarks>
    /// <para>
    /// This method uses deferred execution: the actual file system operations
    /// are performed when <see cref="Build"/> is called, not when this method is called.
    /// </para>
    /// <para>
    /// The <paramref name="rootRelativePath"/> is added to the <see cref="Content"/> collection.
    /// </para>
    /// </remarks>
    IFileSystemBuilder Add(string rootRelativePath, Action<string, IFileSystemBuilder> additionalAction);

    /// <summary>
    /// Executes all queued file system creation operations and returns the resulting file system.
    /// </summary>
    /// <returns>An <see cref="IFileSystem"/> instance representing the created file system.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the root directory has not been set via <see cref="AddRoot"/>.
    /// </exception>
    /// <exception cref="IOException">
    /// Thrown when file system operations (create directory, create file, etc.) fail.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method performs the following actions:
    /// </para>
    /// <list type="number">
    /// <item><description>Creates the root directory if it doesn't exist</description></item>
    /// <item><description>Executes all actions added via <see cref="Add"/> in the order they were added</description></item>
    /// <item><description>If any exception occurs, attempts to clean up the entire file structure</description></item>
    /// <item><description>Returns a ready-to-use <see cref="IFileSystem"/> instance</description></item>
    /// </list>
    /// <para>
    /// After calling this method, the builder should not be reused.
    /// </para>
    /// </remarks>
    IFileSystem Build();
}