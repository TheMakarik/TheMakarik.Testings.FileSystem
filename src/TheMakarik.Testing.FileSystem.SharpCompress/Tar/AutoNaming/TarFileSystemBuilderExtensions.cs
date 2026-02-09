using System;
using System.IO;
using JetBrains.Annotations;
using TheMakarik.Testing.FileSystem.AutoNaming;

namespace TheMakarik.Testing.FileSystem.SharpCompress.Tar.AutoNaming;

// File: TarFileSystemBuilderAutoNamingExtensions.cs

/// <summary>
/// Extension methods for <see cref="ITarFileSystemBuilder"/> to support automatic name generation
/// when adding files and directories inside a tar archive.
/// </summary>
/// <remarks>
/// Uses a separate generator instance from the root file system to avoid interference.
/// </remarks>
[PublicAPI]
public static class TarFileSystemBuilderAutoNamingExtensions
{
    private const string TarInnerGeneratorKey = "auto_naming::generator";

    /// <summary>
    /// Attaches a predefined name generation strategy for files/directories inside the tar archive.
    /// </summary>
    /// <param name="builder">The tar archive builder.</param>
    /// <param name="type">The name generation strategy.</param>
    /// <param name="seed">Optional seed (only used by <see cref="NameGenerationType.RandomNumber"/>).</param>
    /// <returns>The same builder for fluent chaining.</returns>
    public static ITarFileSystemBuilder AddNameGenerator(this ITarFileSystemBuilder builder, NameGenerationType type, int? seed = null)
    {
        builder.Properties[TarInnerGeneratorKey] = new NamingConfiguration
        {
            GenerateFunction =
            NamingInfo = new NamingInfo { RandomSeed = seed }
        };

        return builder;
    }

    /// <summary>
    /// Adds a file inside the tar archive using the current automatic name generator.
    /// </summary>
    /// <param name="builder">The tar archive builder.</param>
    /// <param name="extension">File extension (with dot, e.g. ".txt").</param>
    /// <param name="content">Optional content of the file.</param>
    /// <returns>The same builder for fluent chaining.</returns>
    public static ITarFileSystemBuilder AddFileWithNameGenerating(this ITarFileSystemBuilder builder, string extension, string? content = null)
    {
        var name = GenerateInnerName(builder, extension);
        return content is null ? builder.AddFile(name) : builder.AddFile(name, content);
    }

    /// <summary>
    /// Adds a file with automatic name generation and returns its relative path inside the archive.
    /// </summary>
    public static ITarFileSystemBuilder AddFileWithNameGenerating(this ITarFileSystemBuilder builder, string extension, out string entryRelativePath, string? content = null)
    {
        var name = GenerateInnerName(builder, extension);
        entryRelativePath = Path.Combine(builder.Prefix, name).Replace("\\", "/");
        return content is null ? builder.AddFile(name) : builder.AddFile(name, content);
    }

    /// <summary>
    /// Adds multiple files with automatically generated names inside the tar archive.
    /// </summary>
    public static ITarFileSystemBuilder AddFilesWithNameGenerating(this ITarFileSystemBuilder builder, string extension, int count, string? content = null)
    {
        for (int i = 0; i < count; i++)
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
    public static ITarFileSystemBuilder AddDirectoryWithNameGenerating(
        this ITarFileSystemBuilder builder,
        Func<ITarFileSystemBuilder, ITarFileSystemBuilder> createDirectoryContent)
    {
        var name = GenerateInnerName(builder, string.Empty);
        return builder.AddDirectory(name, createDirectoryContent);
    }

    /// <summary>
    /// Adds a directory with automatic name generation and returns its relative path.
    /// </summary>
    public static ITarFileSystemBuilder AddDirectoryWithNameGenerating(
        this ITarFileSystemBuilder builder,
        out string directoryRelativePath,
        Func<ITarFileSystemBuilder, ITarFileSystemBuilder> createDirectoryContent)
    {
        var name = GenerateInnerName(builder, string.Empty);
        directoryRelativePath = Path.Combine(builder.Prefix, name).Replace("\\", "/");
        return builder.AddDirectory(name, createDirectoryContent);
    }

    private static string GenerateInnerName(ITarFileSystemBuilder builder, string extension)
    {
        if (!builder.Properties.TryGetValue(TarInnerGeneratorKey, out var obj) || obj is not NamingConfiguration config)
            throw new InvalidOperationException("Inner tar name generator was not added");

        config.NamingInfo.Extension = extension;
        return config.GenerateFunction(config.NamingInfo);
    }

    private static string GetRandomName()
    {
        return Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
    }
}