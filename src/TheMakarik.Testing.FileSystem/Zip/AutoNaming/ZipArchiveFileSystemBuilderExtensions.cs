using System;
using System.IO;
using JetBrains.Annotations;
using TheMakarik.Testing.FileSystem.AutoNaming;
using TheMakarik.Testing.FileSystem.Core;

namespace TheMakarik.Testing.FileSystem.Zip.AutoNaming;

/// <summary>
/// Extension methods for <see cref="IZipArchiveFileSystemBuilder"/> that enable automatic
/// name generation for entries inside a zip archive (files and directories).
/// </summary>
/// <remarks>
/// Uses an internal <see cref="NamingConfiguration"/> stored in <see cref="IZipArchiveFileSystemBuilder.Properties"/>
/// under a dedicated key, so it does not interfere with root <see cref="IFileSystemBuilder"/> generators.
/// </remarks>
[PublicAPI]
public static class ZipArchiveFileSystemBuilderAutoNamingExtensions
{
    private const string ZipInnerGeneratorKey = "auto_naming::generator";

    /// <summary>
    /// Attaches a predefined name generation strategy for entries inside the zip archive.
    /// </summary>
    /// <param name="builder">The zip archive builder.</param>
    /// <param name="type">The name generation strategy.</param>
    /// <param name="seed">Optional seed (mainly used by random-based strategies).</param>
    /// <returns>The same builder for fluent chaining.</returns>
    public static IZipArchiveFileSystemBuilder AddNameGenerator(this IZipArchiveFileSystemBuilder builder, NameGenerationType type, int? seed = null)
    {
        builder.Properties[ZipInnerGeneratorKey] = new NamingConfiguration
        {
            GenerateFunction = NamingConfiguration.CreateGeneratingFunction(type),
            NamingInfo = new NamingInfo { RandomSeed = seed }
        };

        return builder;
    }

    /// <summary>
    /// Resets the internal counters of the zip name generator while keeping the same strategy and seed.
    /// </summary>
    /// <param name="builder">The zip archive builder.</param>
    /// <returns>The same builder for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no inner zip name generator was configured via <see cref="AddNameGenerator"/>.
    /// </exception>
    public static IZipArchiveFileSystemBuilder RefreshGenerator(this IZipArchiveFileSystemBuilder builder)
    {
        if (!builder.Properties.TryGetValue(ZipInnerGeneratorKey, out var obj) || obj is not NamingConfiguration config)
            throw new InvalidOperationException("Inner zip name generator was not added");

        builder.Properties[ZipInnerGeneratorKey] = new NamingConfiguration
        {
            GenerateFunction = config.GenerateFunction,
            NamingInfo = new NamingInfo
            {
                RandomSeed = config.NamingInfo.RandomSeed,
                Properties = config.NamingInfo.Properties
            }
        };

        return builder;
    }

    /// <summary>
    /// Adds a file to the zip archive using the current automatic name generator.
    /// </summary>
    /// <param name="builder">The zip archive builder.</param>
    /// <param name="extension">File extension (with dot, e.g. ".txt").</param>
    /// <param name="content">Optional file content. If null, an empty file entry is created.</param>
    public static IZipArchiveFileSystemBuilder AddFileWithNameGenerating(this IZipArchiveFileSystemBuilder builder, string extension, string? content = null)
    {
        var name = GenerateInnerName(builder, extension);
        return content is null
            ? builder.AddFile(name)
            : builder.AddFile(name, content);
    }

    /// <summary>
    /// Adds a file with automatic name generation and returns its relative path inside the archive.
    /// </summary>
    public static IZipArchiveFileSystemBuilder AddFileWithNameGenerating(this IZipArchiveFileSystemBuilder builder, string extension, out string entryRelativePath, string? content = null)
    {
        var name = GenerateInnerName(builder, extension);
        var context = new ZipCreationalContext(name, null!, builder.Prefix);
        entryRelativePath = context.FullEntryName;

        return content is null
            ? builder.AddFile(name)
            : builder.AddFile(name, content);
    }

    /// <summary>
    /// Adds multiple files with automatically generated names inside the zip archive.
    /// </summary>
    public static IZipArchiveFileSystemBuilder AddFilesWithNameGenerating(this IZipArchiveFileSystemBuilder builder, string extension, int count, string? content = null)
    {
        for (var i = 0; i < count; i++)
        {
            var name = GenerateInnerName(builder, extension);
            if (content is null)
                builder.AddFile(name);
            else
                builder.AddFile(name, content);
        }

        return builder;
    }

    /// <summary>
    /// Adds a directory with an automatically generated name and configures its contents.
    /// </summary>
    public static IZipArchiveFileSystemBuilder AddDirectoryWithNameGenerating(
        this IZipArchiveFileSystemBuilder builder,
        Func<IZipArchiveFileSystemBuilder, IZipArchiveFileSystemBuilder> createDirectoryContent)
    {
        var name = GenerateInnerName(builder, string.Empty);
        return builder.AddDirectory(name, createDirectoryContent);
    }

    /// <summary>
    /// Adds a directory with automatic name generation and returns its relative to path.
    /// </summary>
    public static IZipArchiveFileSystemBuilder AddDirectoryWithNameGenerating(
        this IZipArchiveFileSystemBuilder builder,
        out string directoryRelativePath,
        Func<IZipArchiveFileSystemBuilder, IZipArchiveFileSystemBuilder> createDirectoryContent)
    {
        var name = GenerateInnerName(builder, string.Empty);
        var context = new ZipCreationalContext(name, null!, builder.Prefix);
        directoryRelativePath = context.FullEntryName.EndsWith("/")
            ? context.FullEntryName
            : context.FullEntryName + "/";

        return builder.AddDirectory(name, createDirectoryContent);
    }

    private static string GenerateInnerName(IZipArchiveFileSystemBuilder builder, string extension)
    {
        if (!builder.Properties.TryGetValue(ZipInnerGeneratorKey, out var obj) || obj is not NamingConfiguration config)
            throw new InvalidOperationException("Zip name generator was not added");

        config.NamingInfo.Extension = extension;
        return config.GenerateFunction(config.NamingInfo);
    }
}

