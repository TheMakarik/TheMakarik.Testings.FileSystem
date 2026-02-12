using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using JetBrains.Annotations;
using SharpCompress.Archives.Tar;
using SharpCompress.Writers;

namespace TheMakarik.Testing.FileSystem.SharpCompress.Tar;

/// <summary>
/// Extension methods for <see cref="ITarFileSystemBuilder"/> to add files and directories to tar archives.
/// </summary>
[PublicAPI]
public static class TarFileSystemBuilderExtensions
{
    /// <summary>
    /// Adds an empty file entry to the tar archive.
    /// </summary>
    /// <param name="builder">The tar builder.</param>
    /// <param name="fileName">The file name in the archive.</param>
    /// <returns>The same builder for chaining.</returns>
    public static ITarFileSystemBuilder AddFile(this ITarFileSystemBuilder builder, string fileName)
    {
        return builder.Add(fileName, context =>
        {
            using var memoryStream = new MemoryStream();
            context.Archive.Write(context.FullEntryName, memoryStream, DateTime.Now);
        });
    }

    /// <summary>
    /// Adds a file entry with content to the tar archive.
    /// </summary>
    /// <param name="builder">The tar builder.</param>
    /// <param name="fileName">The file name in the archive.</param>
    /// <param name="content">The text content.</param>
    /// <returns>The same builder for chaining.</returns>
    public static ITarFileSystemBuilder AddFile(this ITarFileSystemBuilder builder, string fileName, string content)
    {
        return builder.Add(fileName, context =>
        {
            using var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(content);
            }
            memoryStream.Position = 0;
            context.Archive.Write(context.FullEntryName, memoryStream, DateTime.Now);
        });
    }

    /// <summary>
    /// Adds a file entry with content from a <see cref="Stream"/> to the tar archive.
    /// </summary>
    /// <param name="builder">The tar builder.</param>
    /// <param name="fileName">The file name in the archive.</param>
    /// <param name="contentStream">The stream containing file content.</param>
    /// <param name="lastModified">Optional last modified date. If null, uses current date.</param>
    /// <returns>The same builder for chaining.</returns>
    public static ITarFileSystemBuilder AddFile(this ITarFileSystemBuilder builder, string fileName, Stream contentStream, DateTime? lastModified = null)
    {
        if (contentStream is null) throw new ArgumentNullException(nameof(contentStream));

        return builder.Add(fileName, context =>
        {
            if (contentStream.CanSeek)
                contentStream.Position = 0;

            context.Archive.Write(context.FullEntryName, contentStream, lastModified ?? DateTime.Now);
        });
    }

    /// <summary>
    /// Adds a file entry with content and returns its relative path in the archive.
    /// </summary>
    /// <param name="builder">The tar builder.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="entryRelativePath">Out relative path in archive.</param>
    /// <param name="content">The text content.</param>
    /// <returns>The same builder for chaining.</returns>
    public static ITarFileSystemBuilder AddFile(this ITarFileSystemBuilder builder, string fileName, out string entryRelativePath, string content)
    {
        var dummyContext = new TarCreationalContext(fileName, null!, builder.Prefix);
        entryRelativePath = dummyContext.FullEntryName;
        return builder.AddFile(fileName, content);
    }

    /// <summary>
    /// Adds a file entry with content from a <see cref="Stream"/> and returns its relative path in the archive.
    /// </summary>
    /// <param name="builder">The tar builder.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="entryRelativePath">Out relative path in archive.</param>
    /// <param name="contentStream">The stream containing file content.</param>
    /// <param name="lastModified">Optional last modified date. If null, uses current date.</param>
    /// <returns>The same builder for chaining.</returns>
    public static ITarFileSystemBuilder AddFile(this ITarFileSystemBuilder builder, string fileName, out string entryRelativePath, Stream contentStream, DateTime? lastModified = null)
    {
        if (contentStream is null) throw new ArgumentNullException(nameof(contentStream));

        var dummyContext = new TarCreationalContext(fileName, null!, builder.Prefix);
        entryRelativePath = dummyContext.FullEntryName;
        return builder.AddFile(fileName, contentStream, lastModified);
    }

    /// <summary>
    /// Adds multiple empty file entries to the tar archive.
    /// </summary>
    /// <param name="builder">The tar builder.</param>
    /// <param name="fileNames">Array of file names.</param>
    /// <returns>The same builder for chaining.</returns>
    public static ITarFileSystemBuilder AddFiles(this ITarFileSystemBuilder builder, string[] fileNames)
    {
        foreach (var fileName in fileNames)
        {
            builder.AddFile(fileName);
        }
        return builder;
    }

    /// <summary>
    /// Adds multiple file entries with the same content from a <see cref="Stream"/>.
    /// </summary>
    /// <param name="builder">The tar builder.</param>
    /// <param name="fileNames">Array of file names.</param>
    /// <param name="contentStream">The stream containing file content.</param>
    /// <param name="lastModified">Optional last modified date. If null, uses current date.</param>
    /// <returns>The same builder for chaining.</returns>
    public static ITarFileSystemBuilder AddFiles(this ITarFileSystemBuilder builder, string[] fileNames, Stream contentStream, DateTime? lastModified = null)
    {
        ArgumentNullException.ThrowIfNull(contentStream);

        using var memory = new MemoryStream();
        contentStream.CopyTo(memory);
        var buffer = memory.ToArray();

        foreach (var fileName in fileNames)
        {
            builder.Add(fileName, context =>
            {
                using var copy = new MemoryStream(buffer, writable: false);
                context.Archive.Write(context.FullEntryName, copy, lastModified ?? DateTime.Now);
            });
        }

        return builder;
    }

    /// <summary>
    /// Adds multiple file entries with the same content.
    /// </summary>
    /// <param name="builder">The tar builder.</param>
    /// <param name="fileNames">Array of file names.</param>
    /// <param name="content">The common text content.</param>
    /// <returns>The same builder for chaining.</returns>
    public static ITarFileSystemBuilder AddFiles(this ITarFileSystemBuilder builder, string[] fileNames, string content)
    {
        foreach (var fileName in fileNames)
        {
            builder.AddFile(fileName, content);
        }
        return builder;
    }

    /// <summary>
    /// Adds multiple file entries with content and returns their relative paths.
    /// </summary>
    /// <param name="builder">The tar builder.</param>
    /// <param name="fileNames">Array of file names.</param>
    /// <param name="entriesRelativePaths">Out array of relative paths.</param>
    /// <param name="content">The common text content.</param>
    /// <returns>The same builder for chaining.</returns>
    public static ITarFileSystemBuilder AddFiles(this ITarFileSystemBuilder builder, string[] fileNames, out string[] entriesRelativePaths, string content)
    {
        entriesRelativePaths = new string[fileNames.Length];
        for (int i = 0; i < fileNames.Length; i++)
        {
            var dummyContext = new TarCreationalContext(fileNames[i], null!, builder.Prefix);
            entriesRelativePaths[i] = dummyContext.FullEntryName;
            builder.AddFile(fileNames[i], content);
        }
        return builder;
    }

    /// <summary>
    /// Adds a directory entry to the tar archive with nested content.
    /// </summary>
    /// <param name="builder">The tar builder.</param>
    /// <param name="directoryName">The directory name.</param>
    /// <param name="createDirectory">Function to build nested content.</param>
    /// <returns>The same builder for chaining.</returns>
    /// <remarks>
    /// Tar directories are implicit; this adds a prefix for nested entries.
    /// </remarks>
    public static ITarFileSystemBuilder AddDirectory(this ITarFileSystemBuilder builder, string directoryName, Func<ITarFileSystemBuilder, ITarFileSystemBuilder> createDirectory)
    {
        return builder.Add(directoryName, context =>
        {
            var directoryPath = context.FullEntryName;
            if (!directoryPath.EndsWith("/"))
            {
                directoryPath += "/";
            }
            
            using var emptyStream = new MemoryStream();
            context.Archive.WriteDirectory(directoryPath, DateTime.Now);
            
            var nestedBuilder = new TarFileSystemBuilder(builder.Root, 
                Path.Combine(builder.Prefix, directoryName).Replace("\\", "/"), 
                context.Archive);
            
            foreach (var property in builder.Properties)
                nestedBuilder.Properties[property.Key] = property.Value;
            
            createDirectory(nestedBuilder).Build();
        });
    }

    /// <summary>
    /// Adds a directory with nested content and returns its relative path.
    /// </summary>
    /// <param name="builder">The tar builder.</param>
    /// <param name="directoryName">The directory name.</param>
    /// <param name="directoryRelativePath">Out relative path.</param>
    /// <param name="createDirectory">Function to build content.</param>
    /// <returns>The same builder for chaining.</returns>
    public static ITarFileSystemBuilder AddDirectory(this ITarFileSystemBuilder builder, string directoryName, out string directoryRelativePath, Func<ITarFileSystemBuilder, ITarFileSystemBuilder> createDirectory)
    {
        var dummyContext = new TarCreationalContext(directoryName, null!, builder.Prefix);
        directoryRelativePath = dummyContext.FullEntryName;
        if (!directoryRelativePath.EndsWith("/"))
            directoryRelativePath += "/";
        return builder.AddDirectory(directoryName, createDirectory);
    }

    /// <summary>
    /// Adds multiple directory entries to the tar archive.
    /// </summary>
    /// <param name="builder">The tar builder.</param>
    /// <param name="directoryNames">Array of directory names.</param>
    /// <param name="createDirectory">Function to build content for each (applied to all).</param>
    /// <returns>The same builder for chaining.</returns>
    public static ITarFileSystemBuilder AddDirectories(this ITarFileSystemBuilder builder, string[] directoryNames, Func<ITarFileSystemBuilder, ITarFileSystemBuilder> createDirectory)
    {
        foreach (var dirName in directoryNames)
        {
            builder.AddDirectory(dirName, createDirectory);
        }
        return builder;
    }

    /// <summary>
    /// Adds multiple directories and returns their relative paths.
    /// </summary>
    /// <param name="builder">The tar builder.</param>
    /// <param name="directoryNames">Array of directory names.</param>
    /// <param name="directoriesRelativePaths">Out array of relative paths.</param>
    /// <param name="createDirectory">Function to build content.</param>
    /// <returns>The same builder for chaining.</returns>
    public static ITarFileSystemBuilder AddDirectories(this ITarFileSystemBuilder builder, string[] directoryNames, out string[] directoriesRelativePaths, Func<ITarFileSystemBuilder, ITarFileSystemBuilder> createDirectory)
    {
        directoriesRelativePaths = new string[directoryNames.Length];
        for (int i = 0; i < directoryNames.Length; i++)
        {
            var dummyContext = new TarCreationalContext(directoryNames[i], null!, builder.Prefix);
            directoriesRelativePaths[i] = dummyContext.FullEntryName;
            if (!directoriesRelativePaths[i].EndsWith("/"))
            {
                directoriesRelativePaths[i] += "/";
            }
            builder.AddDirectory(directoryNames[i], createDirectory);
        }
        return builder;
    }
    
    /// <summary>
    /// Adds a file entry with content from a stream to the tar archive.
    /// </summary>
    /// <param name="builder">The tar builder.</param>
    /// <param name="fileName">The file name in the archive.</param>
    /// <param name="stream">The stream containing file content.</param>
    /// <param name="lastModified">Optional last modified date. If null, uses current date.</param>
    /// <returns>The same builder for chaining.</returns>
    public static ITarFileSystemBuilder AddFileFromStream(this ITarFileSystemBuilder builder, string fileName, Stream stream, DateTime? lastModified = null)
    {
        return builder.Add(fileName, context =>
        {
            context.Archive.Write(context.FullEntryName, stream, lastModified ?? DateTime.Now);
        });
    }

    /// <summary>
    /// Adds multiple file entries with the same content from a <see cref="Stream"/> and returns their relative paths.
    /// </summary>
    /// <param name="builder">The tar builder.</param>
    /// <param name="fileNames">Array of file names.</param>
    /// <param name="entriesRelativePaths">Out array of relative paths.</param>
    /// <param name="contentStream">The stream containing file content.</param>
    /// <param name="lastModified">Optional last modified date. If null, uses current date.</param>
    /// <returns>The same builder for chaining.</returns>
    public static ITarFileSystemBuilder AddFiles(this ITarFileSystemBuilder builder, string[] fileNames, out string[] entriesRelativePaths, Stream contentStream, DateTime? lastModified = null)
    {
        if (contentStream is null) throw new ArgumentNullException(nameof(contentStream));

        entriesRelativePaths = new string[fileNames.Length];

        using var memory = new MemoryStream();
        contentStream.CopyTo(memory);
        var buffer = memory.ToArray();

        for (var i = 0; i < fileNames.Length; i++)
        {
            var dummyContext = new TarCreationalContext(fileNames[i], null!, builder.Prefix);
            entriesRelativePaths[i] = dummyContext.FullEntryName;

            builder.Add(fileNames[i], context =>
            {
                using var copy = new MemoryStream(buffer, writable: false);
                context.Archive.Write(context.FullEntryName, copy, lastModified ?? DateTime.Now);
            });
        }

        return builder;
    }
}