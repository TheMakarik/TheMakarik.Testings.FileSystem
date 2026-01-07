using System;
using System.IO;
using JetBrains.Annotations;
using TheMakarik.Testing.FileSystem.Core;

namespace TheMakarik.Testing.FileSystem;

/// <summary>
/// Provides extension methods for <see cref="IFileSystemBuilder"/> to simplify creation of temporary file structures for integrational tests.
/// </summary>
/// <remarks>
/// This class provides convenient methods for creating temporary files and directories
/// which is ideal for isolated testing of file operations.
/// </remarks>
[PublicAPI]
public static class FileSystemBuilderExtensions
{
    /// <summary>
    /// Sets the root directory in the system folder with the specified name in temp folder.
    /// </summary>
    /// <param name="builder">The <see cref="IFileSystemBuilder"/> instance.</param>
    /// <param name="root">The name of the root directory (will be created in the system temporary folder).</param>
    /// <returns>The same <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    public static IFileSystemBuilder AddInTempRoot(this IFileSystemBuilder builder, string root)
    {
        var tempRootFolder = Path.Combine(Path.GetTempPath(), root);
        return builder.AddRoot(tempRootFolder);
    }

    /// <summary>
    /// Sets the root directory in the temp folder with the specified name and returns the full path to the created directory.
    /// </summary>
    /// <param name="builder">The <see cref="IFileSystemBuilder"/> instance.</param>
    /// <param name="root">The name of the root directory (will be created in the system temporary folder).</param>
    /// <param name="fullPath">The full path to the created root directory.</param>
    /// <returns>The same <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    public static IFileSystemBuilder AddInTempRoot(this IFileSystemBuilder builder, string root, out string fullPath)
    {
        fullPath = Path.Combine(Path.GetTempPath(), root);
        return builder.AddRoot(fullPath);
    }

    /// <summary>
    /// Sets the root directory with a randomly generated name.
    /// </summary>
    /// <param name="builder">The <see cref="IFileSystemBuilder"/> instance.</param>
    /// <returns>The same <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    /// <remarks>
    /// The directory name is generated using <see cref="Path.GetRandomFileName"/>.
    /// Use this method to create unique test environments.
    /// </remarks>
    public static IFileSystemBuilder AddRandomRootName(this IFileSystemBuilder builder)
    {
        return builder.AddRoot(Path.GetRandomFileName());
    }

    /// <summary>
    /// Sets the root directory with a randomly generated name and returns the generated name.
    /// </summary>
    /// <param name="builder">The <see cref="IFileSystemBuilder"/> instance.</param>
    /// <param name="rootName">The generated random name of the root directory.</param>
    /// <returns>The same <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    public static IFileSystemBuilder AddRandomRootName(this IFileSystemBuilder builder, out string rootName)
    {
        rootName = Path.GetRandomFileName();
        return builder.AddRoot(rootName);
    }
    
    /// <summary>
    /// Sets the root directory with a randomly generated name in temp.
    /// </summary>
    /// <param name="builder">The <see cref="IFileSystemBuilder"/> instance.</param>
    /// <returns>The same <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    /// <remarks>
    /// The directory name is generated using <see cref="Path.GetRandomFileName"/>.
    /// Use this method to create unique test environments.
    /// </remarks>
    public static IFileSystemBuilder AddRandomInTempRootName(this IFileSystemBuilder builder)
    {
        return builder.AddInTempRoot(Path.GetRandomFileName());
    }

    /// <summary>
    /// Sets the root directory with a randomly generated name and returns the generated name in temp folder.
    /// </summary>
    /// <param name="builder">The <see cref="IFileSystemBuilder"/> instance.</param>
    /// <param name="fullPath">The generated random full path of the root directory.</param>
    /// <returns>The same <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    public static IFileSystemBuilder AddRandomInTempRootName(this IFileSystemBuilder builder, out string fullPath)
    {
        return builder.AddInTempRoot(Path.GetRandomFileName(), out  fullPath);
    }

    /// <summary>
    /// Adds an empty file at the specified relative path from the root.
    /// </summary>
    /// <param name="builder">The <see cref="IFileSystemBuilder"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the file from the root directory.</param>
    /// <returns>The same <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    /// <remarks>
    /// If the path contains subdirectories, they will be created automatically.
    /// The file is created as empty (0 bytes).
    /// </remarks>
    public static IFileSystemBuilder AddFile(this IFileSystemBuilder builder, string rootRelativePath)
    {
        return builder
            .Add(rootRelativePath, (fullPath, _) => File.Create(fullPath).Dispose());
    }

    /// <summary>
    /// Adds an empty file at the specified relative path from the root and returns the full path to the created file.
    /// </summary>
    /// <param name="builder">The <see cref="IFileSystemBuilder"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the file from the root directory.</param>
    /// <param name="fullPath">The full path to the created file.</param>
    /// <returns>The same <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    public static IFileSystemBuilder AddFile(this IFileSystemBuilder builder,
        string rootRelativePath,
        out string fullPath)
    {
        fullPath = Path.Combine(builder.RootDirectory, rootRelativePath);
        return builder
            .Add(rootRelativePath, (fullPath, _) => File.Create(fullPath).Dispose());
    }

    /// <summary>
    /// Adds a file with the specified content at the relative path from the root.
    /// </summary>
    /// <param name="builder">The <see cref="IFileSystemBuilder"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the file from the root directory.</param>
    /// <param name="content">The file content as a string.</param>
    /// <returns>The same <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    /// <remarks>
    /// Content is written with UTF-8 encoding without BOM.
    /// </remarks>
    public static IFileSystemBuilder AddFile(this IFileSystemBuilder builder,
        string rootRelativePath,
        string content)
    {
        return builder
            .Add(rootRelativePath, (fullPath, _) =>
            {
                using var stream = File.Create(fullPath);
                using var streamWriter = new StreamWriter(stream);
                streamWriter.Write(content);
            });
    }

    /// <summary>
    /// Adds a file with the specified content at the relative path from the root and returns the full path to the created file.
    /// </summary>
    /// <param name="builder">The <see cref="IFileSystemBuilder"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the file from the root directory.</param>
    /// <param name="content">The file content as a string.</param>
    /// <param name="fullPath">The full path to the created file.</param>
    /// <returns>The same <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    public static IFileSystemBuilder AddFile(this IFileSystemBuilder builder,
        string rootRelativePath,
        string content,
        out string fullPath)
    {
        fullPath = Path.Combine(builder.RootDirectory, rootRelativePath);
        return builder.AddFile(rootRelativePath, content);
    }

    /// <summary>
    /// Adds a directory at the specified relative path from the root.
    /// </summary>
    /// <param name="builder">The <see cref="IFileSystemBuilder"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the directory from the root directory.</param>
    /// <param name="directoryContentLazyCreational">
    /// Optional function for lazy creation of directory content.
    /// Receives the full path to the directory and a builder for creating content.
    /// </param>
    /// <returns>The same <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    /// <remarks>
    /// If <paramref name="directoryContentLazyCreational"/> is not specified, an empty directory is created.
    /// If specified, a nested structure of files and folders can be created.
    /// </remarks>
    public static IFileSystemBuilder AddDirectory(this IFileSystemBuilder builder,
        string rootRelativePath,
        Func<string, IFileSystemBuilder, IFileSystemBuilder>? directoryContentLazyCreational = null)
    {
        return builder.Add(rootRelativePath, (fullPath, _) =>
        {
            if (directoryContentLazyCreational is null)
            {
                Directory.CreateDirectory(fullPath);
                return;
            }

            Directory.CreateDirectory(fullPath);
            var directoryContentBuilder = FileSystem.BeginBuilding();
            directoryContentBuilder.AddRoot(fullPath);
            directoryContentLazyCreational.Invoke(fullPath, directoryContentBuilder);
            directoryContentBuilder.Build();
        });
    }

    /// <summary>
    /// Adds a directory at the specified relative path from the root and returns the full path to the created directory.
    /// </summary>
    /// <param name="builder">The <see cref="IFileSystemBuilder"/> instance.</param>
    /// <param name="rootPath">The full path to the created directory.</param>
    /// <param name="rootRelativePath">The relative path to the directory from the root directory.</param>
    /// <param name="directoryContentLazyCreational">
    /// Optional function for lazy creation of directory content.
    /// </param>
    /// <returns>The same <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    public static IFileSystemBuilder AddDirectory(this IFileSystemBuilder builder,
        string rootRelativePath,
        out string rootPath,
        Func<string, IFileSystemBuilder, IFileSystemBuilder>? directoryContentLazyCreational = null)
    {
        rootPath = Path.Combine(builder.RootDirectory, rootRelativePath);
        return builder.AddDirectory(rootRelativePath, directoryContentLazyCreational);
    }

    /// <summary>
    /// Adds multiple directories at the specified relative paths from the root.
    /// </summary>
    /// <param name="builder">The <see cref="IFileSystemBuilder"/> instance.</param>
    /// <param name="directoriesRelativeNames">Array of relative directory paths from the root directory.</param>
    /// <param name="directoryContentLazyCreational">
    /// Optional function for lazy creation of directory content. Applied to each directory.
    /// Receives the full path to the directory and a builder for creating content.
    /// </param>
    /// <returns>The same <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    /// <remarks>
    /// If <paramref name="directoryContentLazyCreational"/> is specified, the same function is applied to all directories.
    /// To create different content for each directory, use multiple <see cref="AddDirectory"/> calls.
    /// </remarks>
    public static IFileSystemBuilder AddDirectories(this IFileSystemBuilder builder,
        string[] directoriesRelativeNames,
        Func<string, IFileSystemBuilder, IFileSystemBuilder>? directoryContentLazyCreational = null)
    {
        foreach (var relativeName in directoriesRelativeNames)
            builder.AddDirectory(relativeName, directoryContentLazyCreational);
        return builder;
    }

    /// <summary>
    /// Adds multiple directories at the specified relative paths from the root and returns their full paths.
    /// </summary>
    /// <param name="builder">The <see cref="IFileSystemBuilder"/> instance.</param>
    /// <param name="directoriesRelativeNames">Array of relative directory paths from the root directory.</param>
    /// <param name="directoriesRelativeFullPaths">Array of full paths to the created directories.</param>
    /// <param name="directoryContentLazyCreational">
    /// Optional function for lazy creation of directory content. Applied to each directory.
    /// </param>
    /// <returns>The same <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    public static IFileSystemBuilder AddDirectories(this IFileSystemBuilder builder,
        string[] directoriesRelativeNames,
        out string[] directoriesRelativeFullPaths,
        Func<string, IFileSystemBuilder, IFileSystemBuilder>? directoryContentLazyCreational = null)
    {
        directoriesRelativeFullPaths = new string[directoriesRelativeNames.Length];

        for (var i = 0; i < directoriesRelativeNames.Length; i++)
        {
            var relativePath = directoriesRelativeNames[i];
            var fullPath = Path.Combine(builder.RootDirectory, relativePath);
            directoriesRelativeFullPaths[i] = fullPath;

            builder.AddDirectory(relativePath, directoryContentLazyCreational);
        }

        return builder;
    }

    /// <summary>
    /// Adds multiple empty files at the specified relative paths from the root.
    /// </summary>
    /// <param name="builder">The <see cref="IFileSystemBuilder"/> instance.</param>
    /// <param name="filesRelativeNames">Array of relative file paths from the root directory.</param>
    /// <returns>The same <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    /// <remarks>
    /// All files are created as empty (0 bytes).
    /// If paths contain subdirectories, they will be created automatically.
    /// </remarks>
    public static IFileSystemBuilder AddFiles(this IFileSystemBuilder builder,
        string[] filesRelativeNames)
    {
        foreach (var relativeName in filesRelativeNames)
            builder.AddFile(relativeName);
        return builder;
    }

    /// <summary>
    /// Adds multiple empty files at the specified relative paths from the root and returns their full paths.
    /// </summary>
    /// <param name="builder">The <see cref="IFileSystemBuilder"/> instance.</param>
    /// <param name="filesRelativeNames">Array of relative file paths from the root directory.</param>
    /// <param name="filesFullPaths">Array of full paths to the created files.</param>
    /// <returns>The same <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    public static IFileSystemBuilder AddFiles(this IFileSystemBuilder builder,
        string[] filesRelativeNames,
        out string[] filesFullPaths)
    {
        filesFullPaths = new string[filesRelativeNames.Length];

        for (var i = 0; i < filesRelativeNames.Length; i++)
        {
            var relativePath = filesRelativeNames[i];
            var fullPath = Path.Combine(builder.RootDirectory, relativePath);
            filesFullPaths[i] = fullPath;

            builder.AddFile(relativePath);
        }

        return builder;
    }

    /// <summary>
    /// Adds multiple files with the same content at the specified relative paths from the root.
    /// </summary>
    /// <param name="builder">The <see cref="IFileSystemBuilder"/> instance.</param>
    /// <param name="filesRelativeNames">Array of relative file paths from the root directory.</param>
    /// <param name="content">The content to write to all files.</param>
    /// <returns>The same <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    /// <remarks>
    /// The same content is written to all specified files.
    /// Content is written with UTF-8 encoding without BOM.
    /// </remarks>
    public static IFileSystemBuilder AddFiles(this IFileSystemBuilder builder,
        string[] filesRelativeNames,
        string content)
    {
        foreach (var relativeName in filesRelativeNames)
            builder.AddFile(relativeName, content);
        return builder;
    }

    /// <summary>
    /// Adds multiple files with the same content at the specified relative paths from the root and returns their full paths.
    /// </summary>
    /// <param name="builder">The <see cref="IFileSystemBuilder"/> instance.</param>
    /// <param name="filesRelativeNames">Array of relative file paths from the root directory.</param>
    /// <param name="content">The content to write to all files.</param>
    /// <param name="filesFullPaths">Array of full paths to the created files.</param>
    /// <returns>The same <see cref="IFileSystemBuilder"/> instance for method chaining.</returns>
    public static IFileSystemBuilder AddFiles(this IFileSystemBuilder builder,
        string[] filesRelativeNames,
        string content,
        out string[] filesFullPaths)
    {
        filesFullPaths = new string[filesRelativeNames.Length];

        for (var i = 0; i < filesRelativeNames.Length; i++)
        {
            var relativePath = filesRelativeNames[i];
            var fullPath = Path.Combine(builder.RootDirectory, relativePath);
            filesFullPaths[i] = fullPath;

            builder.AddFile(relativePath, content);
        }

        return builder;
    }
}