using System;
using System.IO;
using System.IO.Compression;

namespace TheMakarik.Testing.FileSystem.Zip;

/// <summary>
/// Provides extension methods for <see cref="IZipArchiveFileSystemBuilder"/> to simplify creation of zip archive contents.
/// </summary>
public static class ZipArchiveBuilderExtensions
{
    /// <summary>
    /// Adds an empty file entry to the zip archive.
    /// </summary>
    /// <param name="builder">The <see cref="IZipArchiveFileSystemBuilder"/> instance.</param>
    /// <param name="fileName">The name of the file entry to add to the archive.</param>
    /// <param name="compressionLevel">The compression level to use for the file entry. Default is <see cref="CompressionLevel.Optimal"/>.</param>
    /// <returns>The same <see cref="IZipArchiveFileSystemBuilder"/> instance for method chaining.</returns>
    /// <remarks>
    /// The file entry will be created with the current directory prefix. The entry will be empty (0 bytes).
    /// </remarks>
    public static IZipArchiveFileSystemBuilder AddFile(this IZipArchiveFileSystemBuilder builder, string fileName, CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        return builder.Add(fileName, (context) =>
        {
            context.Archive.CreateEntry(context.FullEntryName, compressionLevel);
        });
    }
    
    /// <summary>
    /// Adds a file entry with content to the zip archive.
    /// </summary>
    /// <param name="builder">The <see cref="IZipArchiveFileSystemBuilder"/> instance.</param>
    /// <param name="fileName">The name of the file entry to add to the archive.</param>
    /// <param name="content">The text content to write to the file entry.</param>
    /// <param name="compressionLevel">The compression level to use for the file entry. Default is <see cref="CompressionLevel.Optimal"/>.</param>
    /// <returns>The same <see cref="IZipArchiveFileSystemBuilder"/> instance for method chaining.</returns>
    /// <remarks>
    /// The content is written to a temporary file and then added to the archive with the specified compression level.
    /// The temporary file is automatically cleaned up after being added to the archive.
    /// </remarks>
    public static IZipArchiveFileSystemBuilder AddFile(this IZipArchiveFileSystemBuilder builder, string fileName, string content, CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        return builder.Add(fileName, (context) =>
        {
            var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            using (var stream = File.Create(tempFilePath))
            {
                using var streamWriter = new StreamWriter(stream);
                streamWriter.Write(content);
            }
            
            context.Archive.CreateEntryFromFile(tempFilePath, context.FullEntryName, compressionLevel);
            File.Delete(tempFilePath);
        });
    }

    /// <summary>
    /// Adds a file entry with content from a <see cref="Stream"/> to the zip archive.
    /// </summary>
    /// <param name="builder">The <see cref="IZipArchiveFileSystemBuilder"/> instance.</param>
    /// <param name="fileName">The name of the file entry to add to the archive.</param>
    /// <param name="contentStream">The stream containing file content.</param>
    /// <param name="compressionLevel">The compression level to use for the file entry. Default is <see cref="CompressionLevel.Optimal"/>.</param>
    /// <returns>The same <see cref="IZipArchiveFileSystemBuilder"/> instance for method chaining.</returns>
    public static IZipArchiveFileSystemBuilder AddFile(this IZipArchiveFileSystemBuilder builder, string fileName, Stream contentStream, CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        if (contentStream is null) throw new ArgumentNullException(nameof(contentStream));

        return builder.Add(fileName, context =>
        {
            var entry = context.Archive.CreateEntry(context.FullEntryName, compressionLevel);
            using var entryStream = entry.Open();

            if (contentStream.CanSeek)
                contentStream.Position = 0;

            contentStream.CopyTo(entryStream);
        });
    }

    /// <summary>
    /// Adds a file entry with content to the zip archive and returns the relative path to the entry within the archive.
    /// </summary>
    /// <param name="builder">The <see cref="IZipArchiveFileSystemBuilder"/> instance.</param>
    /// <param name="fileName">The name of the file entry to add to the archive.</param>
    /// <param name="entryRelativePath">The relative path to the entry within the zip archive.</param>
    /// <param name="content">The text content to write to the file entry.</param>
    /// <param name="compressionLevel">The compression level to use for the file entry. Default is <see cref="CompressionLevel.Optimal"/>.</param>
    /// <returns>The same <see cref="IZipArchiveFileSystemBuilder"/> instance for method chaining.</returns>
    public static IZipArchiveFileSystemBuilder AddFile(this IZipArchiveFileSystemBuilder builder, string fileName, out string entryRelativePath, string content, CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        var context = new ZipCreationalContext(fileName, null!, builder.Prefix);
        entryRelativePath = context.FullEntryName;
        
        return builder.AddFile(fileName, content, compressionLevel);
    }

    /// <summary>
    /// Adds a file entry with content from a <see cref="Stream"/> to the zip archive and returns the relative path to the entry within the archive.
    /// </summary>
    /// <param name="builder">The <see cref="IZipArchiveFileSystemBuilder"/> instance.</param>
    /// <param name="fileName">The name of the file entry to add to the archive.</param>
    /// <param name="entryRelativePath">The relative path to the entry within the zip archive.</param>
    /// <param name="contentStream">The stream containing file content.</param>
    /// <param name="compressionLevel">The compression level to use for the file entry. Default is <see cref="CompressionLevel.Optimal"/>.</param>
    /// <returns>The same <see cref="IZipArchiveFileSystemBuilder"/> instance for method chaining.</returns>
    public static IZipArchiveFileSystemBuilder AddFile(this IZipArchiveFileSystemBuilder builder, string fileName, out string entryRelativePath, Stream contentStream, CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        if (contentStream is null) throw new ArgumentNullException(nameof(contentStream));

        var context = new ZipCreationalContext(fileName, null!, builder.Prefix);
        entryRelativePath = context.FullEntryName;
        
        return builder.AddFile(fileName, contentStream, compressionLevel);
    }

    /// <summary>
    /// Adds multiple empty file entries to the zip archive.
    /// </summary>
    /// <param name="builder">The <see cref="IZipArchiveFileSystemBuilder"/> instance.</param>
    /// <param name="fileNames">The names of the file entries to add to the archive.</param>
    /// <param name="compressionLevel">The compression level to use for the file entries. Default is <see cref="CompressionLevel.Optimal"/>.</param>
    /// <returns>The same <see cref="IZipArchiveFileSystemBuilder"/> instance for method chaining.</returns>
    public static IZipArchiveFileSystemBuilder AddFiles(this IZipArchiveFileSystemBuilder builder, string[] fileNames, CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        foreach (var fileName in fileNames)
        {
            builder.AddFile(fileName, compressionLevel);
        }
        return builder;
    }

    /// <summary>
    /// Adds multiple file entries with the same content from a <see cref="Stream"/> to the zip archive.
    /// </summary>
    /// <param name="builder">The <see cref="IZipArchiveFileSystemBuilder"/> instance.</param>
    /// <param name="fileNames">The names of the file entries to add to the archive.</param>
    /// <param name="contentStream">The stream containing file content.</param>
    /// <param name="compressionLevel">The compression level to use for the file entries. Default is <see cref="CompressionLevel.Optimal"/>.</param>
    /// <returns>The same <see cref="IZipArchiveFileSystemBuilder"/> instance for method chaining.</returns>
    public static IZipArchiveFileSystemBuilder AddFiles(this IZipArchiveFileSystemBuilder builder, string[] fileNames, Stream contentStream, CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        if (contentStream is null) throw new ArgumentNullException(nameof(contentStream));

        using var memory = new MemoryStream();
        contentStream.CopyTo(memory);
        var buffer = memory.ToArray();
        
        foreach (var fileName in fileNames)
        {
            builder.Add(fileName, context =>
            {
                var entry = context.Archive.CreateEntry(context.FullEntryName, compressionLevel);
                using var entryStream = entry.Open();
                using var copy = new MemoryStream(buffer, writable: false);
                copy.CopyTo(entryStream);
            });
        }

        return builder;
    }

    /// <summary>
    /// Adds multiple empty file entries to the zip archive and returns their relative paths within the archive.
    /// </summary>
    /// <param name="builder">The <see cref="IZipArchiveFileSystemBuilder"/> instance.</param>
    /// <param name="fileNames">The names of the file entries to add to the archive.</param>
    /// <param name="entriesRelativePaths">The relative paths to the entries within the zip archive.</param>
    /// <param name="compressionLevel">The compression level to use for the file entries. Default is <see cref="CompressionLevel.Optimal"/>.</param>
    /// <returns>The same <see cref="IZipArchiveFileSystemBuilder"/> instance for method chaining.</returns>
    public static IZipArchiveFileSystemBuilder AddFiles(this IZipArchiveFileSystemBuilder builder, string[] fileNames, out string[] entriesRelativePaths, CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        entriesRelativePaths = new string[fileNames.Length];
        
        for (int i = 0; i < fileNames.Length; i++)
        {
            var context = new ZipCreationalContext(fileNames[i], null!, builder.Prefix);
            entriesRelativePaths[i] = context.FullEntryName;
            
            builder.AddFile(fileNames[i], compressionLevel);
        }
        
        return builder;
    }

    /// <summary>
    /// Adds multiple file entries with the same content to the zip archive.
    /// </summary>
    /// <param name="builder">The <see cref="IZipArchiveFileSystemBuilder"/> instance.</param>
    /// <param name="fileNames">The names of the file entries to add to the archive.</param>
    /// <param name="content">The text content to write to all file entries.</param>
    /// <param name="compressionLevel">The compression level to use for the file entries. Default is <see cref="CompressionLevel.Optimal"/>.</param>
    /// <returns>The same <see cref="IZipArchiveFileSystemBuilder"/> instance for method chaining.</returns>
    public static IZipArchiveFileSystemBuilder AddFiles(this IZipArchiveFileSystemBuilder builder, string[] fileNames, string content, CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        foreach (var fileName in fileNames)
        {
            builder.AddFile(fileName, content, compressionLevel);
        }
        return builder;
    }

    /// <summary>
    /// Adds multiple file entries with the same content from a <see cref="Stream"/> to the zip archive and returns their relative paths within the archive.
    /// </summary>
    /// <param name="builder">The <see cref="IZipArchiveFileSystemBuilder"/> instance.</param>
    /// <param name="fileNames">The names of the file entries to add to the archive.</param>
    /// <param name="entriesRelativePaths">The relative paths to the entries within the zip archive.</param>
    /// <param name="contentStream">The stream containing file content.</param>
    /// <param name="compressionLevel">The compression level to use for the file entries. Default is <see cref="CompressionLevel.Optimal"/>.</param>
    /// <returns>The same <see cref="IZipArchiveFileSystemBuilder"/> instance for method chaining.</returns>
    public static IZipArchiveFileSystemBuilder AddFiles(this IZipArchiveFileSystemBuilder builder, string[] fileNames, out string[] entriesRelativePaths, Stream contentStream, CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        if (contentStream is null) throw new ArgumentNullException(nameof(contentStream));

        entriesRelativePaths = new string[fileNames.Length];

        using var memory = new MemoryStream();
        contentStream.CopyTo(memory);
        var buffer = memory.ToArray();
        
        for (int i = 0; i < fileNames.Length; i++)
        {
            var context = new ZipCreationalContext(fileNames[i], null!, builder.Prefix);
            entriesRelativePaths[i] = context.FullEntryName;

            builder.Add(fileNames[i], ctx =>
            {
                var entry = ctx.Archive.CreateEntry(ctx.FullEntryName, compressionLevel);
                using var entryStream = entry.Open();
                using var copy = new MemoryStream(buffer, writable: false);
                copy.CopyTo(entryStream);
            });
        }

        return builder;
    }

    /// <summary>
    /// Adds multiple file entries with the same content to the zip archive and returns their relative paths within the archive.
    /// </summary>
    /// <param name="builder">The <see cref="IZipArchiveFileSystemBuilder"/> instance.</param>
    /// <param name="fileNames">The names of the file entries to add to the archive.</param>
    /// <param name="entriesRelativePaths">The relative paths to the entries within the zip archive.</param>
    /// <param name="content">The text content to write to all file entries.</param>
    /// <param name="compressionLevel">The compression level to use for the file entries. Default is <see cref="CompressionLevel.Optimal"/>.</param>
    /// <returns>The same <see cref="IZipArchiveFileSystemBuilder"/> instance for method chaining.</returns>
    public static IZipArchiveFileSystemBuilder AddFiles(this IZipArchiveFileSystemBuilder builder, string[] fileNames, out string[] entriesRelativePaths, string content, CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        entriesRelativePaths = new string[fileNames.Length];
        
        for (int i = 0; i < fileNames.Length; i++)
        {
            var context = new ZipCreationalContext(fileNames[i], null!, builder.Prefix);
            entriesRelativePaths[i] = context.FullEntryName;
            
            builder.AddFile(fileNames[i], content, compressionLevel);
        }
        
        return builder;
    }

    /// <summary>
    /// Adds a directory entry to the zip archive with nested content.
    /// </summary>
    /// <param name="builder">The <see cref="IZipArchiveFileSystemBuilder"/> instance.</param>
    /// <param name="directoryName">The name of the directory entry to add to the archive.</param>
    /// <param name="createDirectory">The function that creates content within the directory.</param>
    /// <returns>The same <see cref="IZipArchiveFileSystemBuilder"/> instance for method chaining.</returns>
    /// <remarks>
    /// The directory entry itself is created as an empty entry, and nested content is added with the directory name as prefix.
    /// </remarks>
    public static IZipArchiveFileSystemBuilder AddDirectory(this IZipArchiveFileSystemBuilder builder, string directoryName, Func<IZipArchiveFileSystemBuilder, IZipArchiveFileSystemBuilder> createDirectory)
    {
        return builder.Add(directoryName, (context) =>
        {
            var directoryBuilder = new ZipArchiveFileSystemBuilder(builder.Root, context.Archive, directoryName);
            
            foreach (var property in builder.Properties)
                directoryBuilder.Properties[property.Key] = property.Value;

            createDirectory(directoryBuilder).Build();
        });
    }

    /// <summary>
    /// Adds a directory entry to the zip archive with nested content and returns the relative path to the directory within the archive.
    /// </summary>
    /// <param name="builder">The <see cref="IZipArchiveFileSystemBuilder"/> instance.</param>
    /// <param name="directoryName">The name of the directory entry to add to the archive.</param>
    /// <param name="directoryRelativePath">The relative path to the directory entry within the zip archive.</param>
    /// <param name="createDirectory">The function that creates content within the directory.</param>
    /// <returns>The same <see cref="IZipArchiveFileSystemBuilder"/> instance for method chaining.</returns>
    public static IZipArchiveFileSystemBuilder AddDirectory(this IZipArchiveFileSystemBuilder builder, string directoryName, out string directoryRelativePath, Func<IZipArchiveFileSystemBuilder, IZipArchiveFileSystemBuilder> createDirectory)
    {
        var context = new ZipCreationalContext(directoryName, null!, builder.Prefix);
        directoryRelativePath = context.FullEntryName;
        
        return builder.AddDirectory(directoryName, createDirectory);
    }

    /// <summary>
    /// Adds multiple directory entries to the zip archive.
    /// </summary>
    /// <param name="builder">The <see cref="IZipArchiveFileSystemBuilder"/> instance.</param>
    /// <param name="directoryNames">The names of the directory entries to add to the archive.</param>
    /// <param name="createDirectory">The function that creates content within each directory.</param>
    /// <returns>The same <see cref="IZipArchiveFileSystemBuilder"/> instance for method chaining.</returns>
    public static IZipArchiveFileSystemBuilder AddDirectories(this IZipArchiveFileSystemBuilder builder, string[] directoryNames, Func<IZipArchiveFileSystemBuilder, IZipArchiveFileSystemBuilder> createDirectory)
    {
        foreach (var directoryName in directoryNames)
        {
            builder.AddDirectory(directoryName, createDirectory);
        }
        return builder;
    }

    /// <summary>
    /// Adds multiple directory entries to the zip archive and returns their relative paths within the archive.
    /// </summary>
    /// <param name="builder">The <see cref="IZipArchiveFileSystemBuilder"/> instance.</param>
    /// <param name="directoryNames">The names of the directory entries to add to the archive.</param>
    /// <param name="directoriesRelativePaths">The relative paths to the directory entries within the zip archive.</param>
    /// <param name="createDirectory">The function that creates content within each directory.</param>
    /// <returns>The same <see cref="IZipArchiveFileSystemBuilder"/> instance for method chaining.</returns>
    public static IZipArchiveFileSystemBuilder AddDirectories(this IZipArchiveFileSystemBuilder builder, string[] directoryNames, out string[] directoriesRelativePaths, Func<IZipArchiveFileSystemBuilder, IZipArchiveFileSystemBuilder> createDirectory)
    {
        directoriesRelativePaths = new string[directoryNames.Length];
        
        for (int i = 0; i < directoryNames.Length; i++)
        {
            var context = new ZipCreationalContext(directoryNames[i], null!, builder.Prefix);
            directoriesRelativePaths[i] = context.FullEntryName;
            
            builder.AddDirectory(directoryNames[i], createDirectory);
        }
        
        return builder;
    }
}