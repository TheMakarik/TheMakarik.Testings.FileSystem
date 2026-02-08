using System;
using System.IO;
using JetBrains.Annotations;
using TheMakarik.Testing.FileSystem.Core;

namespace TheMakarik.Testing.FileSystem.SharpCompress.Tar;

/// <summary>
/// Extension methods for <see cref="IFileSystemBuilder"/> to add tar archives.
/// </summary>
[PublicAPI]
public static class FileSystemBuilderExtensions
{
    /// <summary>
    /// Adds a tar archive to the file system.
    /// </summary>
    /// <param name="builder">The file system builder.</param>
    /// <param name="name">The name or relative path for the tar file.</param>
    /// <param name="buildContentLazy">Function to build the tar content.</param>
    /// <param name="tarPackTo">Compression type for the tar.</param>
    /// <param name="compressionLevel">Compression level (1-9) for GZip. Ignored for others.</param>
    /// <returns>The same builder for chaining.</returns>
    /// <remarks>
    /// The extension is normalized based on <paramref name="tarPackTo"/>.
    /// If an exception occurs during creation, the partial file is deleted.
    /// </remarks>
    public static IFileSystemBuilder AddTar(this IFileSystemBuilder builder, string name, Func<TarFileSystemBuilder, TarFileSystemBuilder> buildContentLazy, TarPackTo tarPackTo = TarPackTo.None, int? compressionLevel = null)
    {
        builder.Add(name, (_, self) =>
        {
            name = NormalizeName(name, tarPackTo);
            var tar = new TarFileSystemBuilder(Path.Combine(self.RootDirectory, name), tarPackTo, compressionLevel);  
            buildContentLazy(tar).Build();
        });
        
        return builder;
    }

    /// <summary>
    /// Adds a tar archive and returns its full path.
    /// </summary>
    /// <param name="builder">The file system builder.</param>
    /// <param name="name">The name or relative path for the tar file.</param>
    /// <param name="fullPath">Out parameter for the full path of the created tar.</param>
    /// <param name="buildContentLazy">Function to build the tar content.</param>
    /// <param name="tarPackTo">Compression type for the tar.</param>
    /// <param name="compressionLevel">Compression level for GZip.</param>
    /// <returns>The same builder for chaining.</returns>
    public static IFileSystemBuilder AddTar(this IFileSystemBuilder builder, string name, out string fullPath, Func<TarFileSystemBuilder, TarFileSystemBuilder> buildContentLazy, TarPackTo tarPackTo = TarPackTo.None, int? compressionLevel = null)
    {
        name = NormalizeName(name, tarPackTo);
        fullPath = Path.Combine(builder.RootDirectory, name);
        return builder.AddTar(name, buildContentLazy, tarPackTo, compressionLevel);
    }

    /// <summary>
    /// Adds multiple tar archives to the file system.
    /// </summary>
    /// <param name="builder">The file system builder.</param>
    /// <param name="names">Array of names or relative paths for the tar files.</param>
    /// <param name="buildContentLazy">Function to build content for each tar (applied to all).</param>
    /// <param name="tarPackTo">Compression type for all tars.</param>
    /// <param name="compressionLevel">Compression level for GZip.</param>
    /// <returns>The same builder for chaining.</returns>
    /// <remarks>
    /// Use separate AddTar calls if different content is needed for each.
    /// </remarks>
    public static IFileSystemBuilder AddTars(this IFileSystemBuilder builder, string[] names, Func<TarFileSystemBuilder, TarFileSystemBuilder> buildContentLazy, TarPackTo tarPackTo = TarPackTo.None, int? compressionLevel = null)
    {
        foreach (var name in names)
        {
            builder.AddTar(name, buildContentLazy, tarPackTo, compressionLevel);
        }
        return builder;
    }

    /// <summary>
    /// Adds multiple tar archives and returns their full paths.
    /// </summary>
    /// <param name="builder">The file system builder.</param>
    /// <param name="names">Array of names or relative paths for the tar files.</param>
    /// <param name="fullPaths">Out array for full paths of created tars.</param>
    /// <param name="buildContentLazy">Function to build content for each.</param>
    /// <param name="tarPackTo">Compression type.</param>
    /// <param name="compressionLevel">Compression level for GZip.</param>
    /// <returns>The same builder for chaining.</returns>
    public static IFileSystemBuilder AddTars(this IFileSystemBuilder builder, string[] names, out string[] fullPaths, Func<TarFileSystemBuilder, TarFileSystemBuilder> buildContentLazy, TarPackTo tarPackTo = TarPackTo.None, int? compressionLevel = null)
    {
        fullPaths = new string[names.Length];
        for (var i = 0; i < names.Length; i++)
        {
            var name = NormalizeName(names[i], tarPackTo);
            fullPaths[i] = Path.Combine(builder.RootDirectory, name);
            builder.AddTar(names[i], buildContentLazy, tarPackTo, compressionLevel);
        }
        return builder;
    }

    private static string NormalizeName(string name, TarPackTo tarPackTo)
    {
        return tarPackTo switch
        {
            TarPackTo.None when Path.GetExtension(name) == ".tar" => name,
            TarPackTo.None => name + ".tar",
            TarPackTo.GZip when name.EndsWith(".tar.gz") => name,
            TarPackTo.GZip when Path.GetExtension(name) == ".tar" => name + ".gz",
            TarPackTo.GZip => name + ".tar.gz",
            TarPackTo.BZip2 when name.EndsWith(".tar.bz2") => name,
            TarPackTo.BZip2 when Path.GetExtension(name) == ".tar" => name + ".bz2",
            TarPackTo.BZip2 => name + ".tar.bz2",
            _ => throw new ArgumentOutOfRangeException(nameof(tarPackTo), tarPackTo, null)
        };
    }
}