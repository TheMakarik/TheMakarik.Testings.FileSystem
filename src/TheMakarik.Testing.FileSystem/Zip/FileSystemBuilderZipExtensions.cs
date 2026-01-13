using System;
using System.IO;
using JetBrains.Annotations;
using TheMakarik.Testing.FileSystem.Core;

namespace TheMakarik.Testing.FileSystem.Zip;

/// <summary>
/// Provides extension methods for <see cref="IFileSystemBuilder"/> to simplify creation of temporary zip archives for integration tests.
/// </summary>
/// <remarks>
/// <para>
/// This class provides convenient methods for creating zip archives within temporary file systems,
/// allowing you to test scenarios involving compressed files and directory structures.
/// </para>
/// <para>
/// All zip archives are created using relative paths from the root directory.
/// The <c>.zip</c> extension is automatically added if not specified.
/// </para>
/// </remarks>
[PublicAPI]
public static class FileSystemBuilderZipExtensions
{
    /// <summary>
    /// Adds a zip archive to the file system at the specified relative path.
    /// </summary>
    /// <param name="builder">The <see cref="IFileSystemBuilder"/> instance.</param>
    /// <param name="archiveName">The relative path to the zip archive from the root directory.</param>
    /// <param name="builderAction">
    /// Optional function for configuring the contents of the zip archive.
    /// Receives an <see cref="IZipArchiveFileSystemBuilder"/> instance for adding files and directories to the archive.
    /// </param>
    /// <returns>The same <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// If <paramref name="builderAction"/> is not specified, an empty zip archive is created.
    /// If specified, you can use the provided builder to add files and directories to the archive.
    /// </para>
    /// <para>
    /// The <c>.zip</c> extension is automatically appended to <paramref name="archiveName"/> if not present.
    /// </para>
    /// <example>
    /// <code>
    /// var fileSystem = FileSystem.BeginBuilding()
    ///     .AddRandomInTempRootName()
    ///     .AddZip("archive.zip", zipBuilder => zipBuilder
    ///         .AddFile("document.txt", "File content")
    ///         .AddDirectory("data", dataBuilder => dataBuilder
    ///             .AddFile("config.json", "{}")))
    ///     .Build();
    /// </code>
    /// </example>
    /// </remarks>
    public static IFileSystemBuilder AddZip(this IFileSystemBuilder builder, string archiveName, Func<IZipArchiveFileSystemBuilder, IZipArchiveFileSystemBuilder>? builderAction)
    {
        Guard.AgainstNull(archiveName, nameof(archiveName));
        
        archiveName = archiveName.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase) ? archiveName : archiveName + ".zip";
        
        return builder.Add(archiveName, (fullPath, b) =>
        {
            var zipArchiveFileSystemBuilder = new ZipArchiveFileSystemBuilder(fullPath);
            builderAction?.Invoke(zipArchiveFileSystemBuilder).Build();
        });
    }

    /// <summary>
    /// Adds a zip archive to the file system and returns the full path to the created archive.
    /// </summary>
    /// <param name="builder">The <see cref="IFileSystemBuilder"/> instance.</param>
    /// <param name="archiveName">The relative path to the zip archive from the root directory.</param>
    /// <param name="archiveFullPath">The full path to the created zip archive.</param>
    /// <param name="builderAction">
    /// Optional function for configuring the contents of the zip archive.
    /// Receives an <see cref="IZipArchiveFileSystemBuilder"/> instance for adding files and directories to the archive.
    /// </param>
    /// <returns>The same <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method provides access to the full path of the created zip archive, which can be useful
    /// when you need to perform additional operations on the archive file itself.
    /// </para>
    /// <para>
    /// The <c>.zip</c> extension is automatically appended to <paramref name="archiveName"/> if not present.
    /// </para>
    /// </remarks>
    public static IFileSystemBuilder AddZip(this IFileSystemBuilder builder, string archiveName,
        out string archiveFullPath,
        Func<IZipArchiveFileSystemBuilder, IZipArchiveFileSystemBuilder>? builderAction)
    {
        archiveName = archiveName.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase) ? archiveName : archiveName + ".zip";
        
        archiveFullPath = Path.Combine(builder.RootDirectory, archiveName);
        return builder.AddZip(archiveName, builderAction);
    }

    /// <summary>
    /// Adds multiple zip archives to the file system at the specified relative paths.
    /// </summary>
    /// <param name="builder">The <see cref="IFileSystemBuilder"/> instance.</param>
    /// <param name="archiveNames">Array of relative paths to the zip archives from the root directory.</param>
    /// <param name="builderAction">
    /// Optional function for configuring the contents of each zip archive.
    /// Receives an <see cref="IZipArchiveFileSystemBuilder"/> instance for adding files and directories to each archive.
    /// The same function is applied to all archives.
    /// </param>
    /// <returns>The same <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// If <paramref name="builderAction"/> is specified, the same function is applied to all zip archives.
    /// To create different content for each archive, use multiple <see cref="AddZip"/> calls.
    /// </para>
    /// <para>
    /// The <c>.zip</c> extension is automatically appended to each archive name if not present.
    /// </para>
    /// </remarks>
    public static IFileSystemBuilder AddZips(this IFileSystemBuilder builder,
        string[] archiveNames,
        Func<IZipArchiveFileSystemBuilder, IZipArchiveFileSystemBuilder>? builderAction = null)
    {
        Guard.AgainstNull(archiveNames, nameof(archiveNames));
        
        foreach (var archiveName in archiveNames)
        {
            builder.AddZip(archiveName, builderAction);
        }
        
        return builder;
    }

    /// <summary>
    /// Adds multiple zip archives to the file system and returns their full paths.
    /// </summary>
    /// <param name="builder">The <see cref="IFileSystemBuilder"/> instance.</param>
    /// <param name="archiveNames">Array of relative paths to the zip archives from the root directory.</param>
    /// <param name="archiveFullPaths">Array of full paths to the created zip archives.</param>
    /// <param name="builderAction">
    /// Optional function for configuring the contents of each zip archive.
    /// Receives an <see cref="IZipArchiveFileSystemBuilder"/> instance for adding files and directories to each archive.
    /// The same function is applied to all archives.
    /// </param>
    /// <returns>The same <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method provides access to the full paths of all created zip archives, which can be useful
    /// when you need to perform batch operations or verify the locations of multiple archives.
    /// </para>
    /// <para>
    /// The <c>.zip</c> extension is automatically appended to each archive name if not present.
    /// </para>
    /// </remarks>
    public static IFileSystemBuilder AddZips(this IFileSystemBuilder builder,
        string[] archiveNames,
        out string[] archiveFullPaths,
        Func<IZipArchiveFileSystemBuilder, IZipArchiveFileSystemBuilder>? builderAction = null)
    {
        Guard.AgainstNull(archiveNames, nameof(archiveNames));
        
        archiveFullPaths = new string[archiveNames.Length];
        
        for (var i = 0; i < archiveNames.Length; i++)
        {
            var archiveName = archiveNames[i];
            var archiveFullPath = Path.Combine(builder.RootDirectory, 
                archiveName.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase) ? archiveName : archiveName + ".zip");
            archiveFullPaths[i] = archiveFullPath;
            
            builder.AddZip(archiveName, builderAction);
        }
        
        return builder;
    }
}