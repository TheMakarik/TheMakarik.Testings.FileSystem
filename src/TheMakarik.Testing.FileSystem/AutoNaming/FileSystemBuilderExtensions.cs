using System;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;
using TheMakarik.Testing.FileSystem.Core;

namespace TheMakarik.Testing.FileSystem.AutoNaming;

[PublicAPI]
/// <summary>
/// Extension methods for <see cref="IFileSystemBuilder"/> that enable automatic name generation for files and directories.
/// 
/// <para>
/// This allows creating multiple test files/directories with unique, predictable names using predefined strategies
/// (<see cref="NameGenerationType"/>) or custom logic. Perfect for load/performance tests or when you need hundreds
/// of files with consistent naming patterns.
/// </para>
/// 
/// <para>
/// <b>Usage example:</b></para>
/// <code>
/// var fs = FileSystem.BeginBuilding()
///     .AddRandomInTempRootName()
///     .AddNameGenerator(NameGenerationType.RandomNameAndCount)  // file1.txt, file2.txt, etc.
///     .AddFilesWithNameGeneraing(".txt", 100, "test content")
///     .AddDirectoryWithNameGenerating(dir => dir.AddFileWithNameGeneraing(".log"))
///     .Build();
/// </code>
/// </summary>
/// <remarks>
/// Name generators track created items automatically via <see cref="IFileSystemBuilder.Added"/> event.
/// Use <see cref="RefreshNameGenerator"/> to reset counters between groups.
/// All random operations can be made deterministic with <paramref name="seed"/> parameter.
/// </remarks>
public static class FileSystemBuilderExtensions
{
    private const string NameGeneratorName = "auto_naming::generator";

    /// <summary>
    /// Attaches a <b>custom</b> name generation function to the builder.
    /// </summary>
    /// <param name="builder">The file system builder to extend.</param>
    /// <param name="function">
    ///     Delegate that receives <see cref="NamingInfo"/> (current counters, extension, created paths)
    ///     and returns the next filename (without path).
    /// </param>
    /// <param name="seed">
    ///     Optional fixed seed for random-based generators. When specified, generation becomes
    ///     fully deterministic and reproducible across test runs.
    /// </param>
    /// <returns>The same <paramref name="builder"/> instance for fluent chaining.</returns>
    /// <remarks>
    /// <para>
    /// The generator automatically tracks all added items via <see cref="IFileSystemBuilder.Added"/> event.
    /// <see cref="NamingInfo.Count"/> and <see cref="NamingInfo.RootFileSystemContent"/> are updated after each addition.
    /// </para>
    /// <para>
    /// <b>Example:</b> Generate names like "myfile.v1.txt", "myfile.v2.txt"...
    /// <code>
    /// .AddCustomNameGenerator(info => $"myfile.v{info.Count + 1}{info.Extension}")
    /// </code>
    /// </para>
    /// </remarks>
    public static IFileSystemBuilder AddCustomNameGenerator(this IFileSystemBuilder builder, Func<NamingInfo, string> function, int? seed = null)
    {
        builder.Properties[NameGeneratorName] = new NamingConfiguration(){ GenerateFunction = function, NamingInfo = new NamingInfo() { RandomSeed = seed, RootFileSystemPath = builder.RootDirectory} };
        builder.Added += (sender, args) => OnAdded(builder, args.FullPath);
        return builder;
    }

    /// <summary>
    /// Attaches one of the predefined name generation strategies from <see cref="NameGenerationType"/>.
    /// </summary>
    /// <param name="builder">The file system builder to extend.</param>
    /// <param name="generationType">Predefined naming pattern to use. See <see cref="NameGenerationType"/> for details.</param>
    /// <param name="seed">
    ///     Optional fixed seed (affects only random-based strategies like <see cref="NameGenerationType.RandomName"/>).
    ///     Makes generation deterministic and reproducible.
    /// </param>
    /// <returns>The same <paramref name="builder"/> instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="generationType"/> is not a valid enum value.
    /// </exception>
    /// <example>
    /// <para>Generate 50 .txt files: "abc123.txt", "xyz789(1).txt", "def456(2).txt"...</para>
    /// <code>
    /// .AddNameGenerator(NameGenerationType.RandomNameAndCount)
    ///  .AddFilesWithNameGeneraing(".txt", 50)
    /// </code>
    /// </example>
    public static IFileSystemBuilder AddNameGenerator(this IFileSystemBuilder builder, NameGenerationType generationType, int? seed = null)
    {
        builder.Properties[NameGeneratorName] =  new NamingConfiguration()
        {
            GenerateFunction = NamingConfiguration.CreateGeneratingFunction(generationType), 
            NamingInfo = new NamingInfo(){RandomSeed = seed, RootFileSystemPath = builder.RootDirectory}
        };
        builder.Added += (sender, args) => OnAdded(builder, args.FullPath);
        return builder;
    }

    /// <summary>
    /// <b>Resets</b> the current name generator counters and content tracking to zero.
    /// </summary>
    /// <param name="builder">The file system builder with active name generator.</param>
    /// <returns>The same <paramref name="builder"/> instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when no name generator was added via <see cref="AddNameGenerator"/> or <see cref="AddCustomNameGenerator"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Useful when creating multiple groups of files with independent numbering:
    /// </para>
    /// <code>
    /// .AddNameGenerator(NameGenerationType.RandomNameAndCount)
    ///  .AddFilesWithNameGeneraing(".txt", 10)        // file1.txt ... file10.txt
    ///  .RefreshNameGenerator()                       // reset counters
    ///  .AddFilesWithNameGeneraing(".log", 5)         // file1.log ... file5.log (separate sequence)
    /// </code>
    /// </remarks>
    public static IFileSystemBuilder RefreshNameGenerator(this IFileSystemBuilder builder)
    {
        ThrowExceptionIfCannotFindNameGenerator(builder);
        
        var generator = builder.Properties[NameGeneratorName] as NamingConfiguration ?? throw new InvalidOperationException("Naming Generator do not added");
        builder.Properties[NameGeneratorName] = new NamingConfiguration(){GenerateFunction = generator.GenerateFunction, NamingInfo = new NamingInfo(){RootFileSystemPath = builder.RootDirectory, RandomSeed = generator.NamingInfo.RandomSeed, Properties = generator.NamingInfo.Properties }};
        return builder;
    }

    /// <summary>
    /// Adds <b>one file</b> with name automatically generated by current generator + specified extension.
    /// </summary>
    /// <param name="builder">The file system builder with active name generator.</param>
    /// <param name="extension">File extension with leading dot (e.g. <c>".txt"</c>, <c>".json"</c>).</param>
    /// <param name="content">Optional content to write into the file. Creates empty file if <see langword="null"/>.</param>
    /// <returns>The same <paramref name="builder"/> instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no name generator was added.</exception>
    public static IFileSystemBuilder AddFileWithNameGeneraing(this IFileSystemBuilder builder, string extension, string? content = null)
    {
        var name = GenerateName(builder, extension);
        return content is null 
            ? builder.AddFile(name) 
            : builder.AddFile(name, content);
    }

    /// <summary>
    /// Adds <b>one file</b> with auto-generated name and returns its full filesystem path.
    /// </summary>
    /// <param name="builder">The file system builder with active name generator.</param>
    /// <param name="extension">File extension with leading dot (e.g. <c>".txt"</c>).</param>
    /// <param name="fullPath">
    ///     <see langword="out"/> parameter receiving absolute path to created file
    ///     (e.g. <c>C:\Temp\abc123.txt</c>).
    /// </param>
    /// <param name="content">Optional file content. Creates empty file if <see langword="null"/>.</param>
    /// <returns>The same <paramref name="builder"/> instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no name generator was added.</exception>
    public static IFileSystemBuilder AddFileWithNameGeneraing(this IFileSystemBuilder builder, string extension, out string fullPath, string? content = null)
    {
        var name = GenerateName(builder, extension);
        return content is null 
            ? builder.AddFile(name, out fullPath) 
            : builder.AddFile(name, content, out fullPath);
    }
    
    /// <summary>
    /// Adds <b>multiple files</b> (same count) with automatically generated names + same extension.
    /// </summary>
    /// <param name="builder">The file system builder with active name generator.</param>
    /// <param name="extension">Extension for all files (with leading dot).</param>
    /// <param name="count">Exact number of files to create (&gt;= 0).</param>
    /// <param name="content">Same content for <b>all</b> files. Use <see langword="null"/> for empty files.</param>
    /// <returns>The same <paramref name="builder"/> instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no name generator was added.</exception>
    /// <remarks>
    /// <para>Each file gets unique name based on current generator state. Counter increments automatically.</para>
    /// <para><b>Performance note:</b> Optimized for bulk creation - generates all names first, then adds files in batch.</para>
    /// </remarks>
    public static IFileSystemBuilder AddFilesWithNameGeneraing(this IFileSystemBuilder builder, string extension, int count, string? content = null)
    {
        var names = new string[count];
        for(var i = 0; i < count; i++)
            names[i] = GenerateName(builder, extension);
        return content is null 
            ? builder.AddFiles(names) 
            : builder.AddFiles(names, content);
    }
    
    /// <summary>
    /// Adds <b>multiple files</b> with auto-generated names and returns array of their full paths.
    /// </summary>
    /// <param name="builder">The file system builder with active name generator.</param>
    /// <param name="extension">Extension for all files (with leading dot).</param>
    /// <param name="fullPaths">
    ///     <see langword="out"/> array with absolute paths to all created files
    ///     (length exactly <paramref name="count"/>).
    /// </param>
    /// <param name="count">Number of files to create.</param>
    /// <param name="content">Same content for all files (or <see langword="null"/> for empty).</param>
    /// <returns>The same <paramref name="builder"/> instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no name generator was added.</exception>
    public static IFileSystemBuilder AddFilesWithNameGeneraing(this IFileSystemBuilder builder, string extension,  out string[] fullPaths, int count, string? content = null)
    {
        var names = new string[count];
        for(var i = 0; i < count; i++)
            names[i] = GenerateName(builder, extension);
        return content is null 
            ? builder.AddFiles(names, out fullPaths) 
            : builder.AddFiles(names, content, out fullPaths);
    }

    /// <summary>
    /// Adds <b>one directory</b> with auto-generated name (no extension) and configures its content.
    /// </summary>
    /// <param name="builder">The file system builder with active name generator.</param>
    /// <param name="createDirectoryContent">
    ///     Delegate receiving generated directory name (relative) and nested builder.
    ///     Use it to add files/subdirectories inside.
    /// </param>
    /// <returns>The same <paramref name="builder"/> instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no name generator was added.</exception>
    /// <example>
    /// <code>
    /// .AddDirectoryWithNameGenerating((name, nested) => 
    ///     nested.AddFileWithNameGeneraing(".log", "directory log"))
    /// </code>
    /// </example>
    public static IFileSystemBuilder AddDirectoryWithNameGenerating(this IFileSystemBuilder builder, Func<string, IFileSystemBuilder, IFileSystemBuilder> createDirectoryContent)
    {
        var name = GenerateName(builder, string.Empty);
        return builder.AddDirectory(name, createDirectoryContent);
    }
    
    /// <summary>
    /// Adds <b>one directory</b> with auto-generated name and returns its full path.
    /// </summary>
    /// <param name="builder">The file system builder with active name generator.</param>
    /// <param name="path">
    ///     <see langword="out"/> parameter with absolute path to created directory.
    /// </param>
    /// <param name="createDirectoryContent">Delegate to configure directory content using nested builder.</param>
    /// <returns>The same <paramref name="builder"/> instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no name generator was added.</exception>
    public static IFileSystemBuilder AddDirectoryWithNameGenerating(this IFileSystemBuilder builder, out string path, Func<string, IFileSystemBuilder, IFileSystemBuilder> createDirectoryContent)
    {
        var name = GenerateName(builder, string.Empty);
        return builder.AddDirectory(name, out path, createDirectoryContent);
    }
    
    /// <summary>
    /// Adds <b>multiple directories</b> (same count) with auto-generated names and same content configuration.
    /// </summary>
    /// <param name="builder">The file system builder with active name generator.</param>
    /// <param name="count">Exact number of directories to create.</param>
    /// <param name="createDirectoryContent">
    ///     Delegate configuring <b>each</b> directory. Receives relative name and nested builder.
    /// </param>
    /// <returns>The same <paramref name="builder"/> instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no name generator was added.</exception>
    /// <remarks>Each directory gets independent nested builder. Generator counter increments for each directory.</remarks>
    public static IFileSystemBuilder AddDirectoriesWithNameGenerating(this IFileSystemBuilder builder, int count, Func<string, IFileSystemBuilder, IFileSystemBuilder> createDirectoryContent)
    {
        var names = new string[count];
        for(var i = 0; i < count; i++)
            names[i] = GenerateName(builder, string.Empty);
        return builder.AddDirectories(names, createDirectoryContent);
    }
    
    /// <summary>
    /// Adds <b>multiple directories</b> with auto-generated names and returns their full paths array.
    /// </summary>
    /// <param name="builder">The file system builder with active name generator.</param>
    /// <param name="count">Number of directories to create.</param>
    /// <param name="paths">
    ///     <see langword="out"/> array containing absolute paths to all created directories
    ///     (length exactly <paramref name="count"/>).
    /// </param>
    /// <param name="createDirectoryContent">Delegate to configure content of each directory.</param>
    /// <returns>The same <paramref name="builder"/> instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no name generator was added.</exception>
    public static IFileSystemBuilder AddDirectoriesWithNameGenerating(this IFileSystemBuilder builder, int count, out string[] paths, Func<string, IFileSystemBuilder, IFileSystemBuilder> createDirectoryContent)
    {
        var names = new string[count];
        for(var i = 0; i < count; i++)
            names[i] = GenerateName(builder, string.Empty);
        return builder.AddDirectories(names, out paths, createDirectoryContent);
    }
    
    /// <summary>
    /// Retrieves the currently active name generator configuration for inspection or debugging.
    /// </summary>
    /// <param name="builder">The file system builder with active name generator.</param>
    /// <returns>
    ///     <see cref="NamingConfiguration"/> instance with current <see cref="NamingConfiguration.GenerateFunction"/>
    ///     and <see cref="NamingInfo"/> state (counters, created paths, etc.).
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when no name generator was previously added with <see cref="AddNameGenerator"/> or <see cref="AddCustomNameGenerator"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Mostly useful for debugging or implementing advanced scenarios where you need to inspect/modify generator state.
    /// </para>
    /// <para>Does <b>not</b> reset or modify the generator - purely read-only access.</para>
    /// </remarks>
    [Pure]
    public static NamingConfiguration GetNameGenerator(this IFileSystemBuilder builder)
    {
        return builder.Properties[NameGeneratorName] as NamingConfiguration ?? throw new InvalidOperationException("Naming Generator do not added");
    }
    
    
    private static string GenerateName(IFileSystemBuilder builder, string extension)
    {
        ThrowExceptionIfCannotFindNameGenerator(builder);
        var generator = builder.GetNameGenerator();
        generator.NamingInfo.Extension = extension;
        return generator.GenerateFunction(generator.NamingInfo);
    }

    private static void OnAdded(this IFileSystemBuilder builder, string fullPath)
    {
        Debug.Assert(builder.Properties.ContainsKey(NameGeneratorName));
        var generator = builder.GetNameGenerator();
        generator.NamingInfo.Count++;
        generator.NamingInfo.RootFileSystemContent.Add(fullPath);
    }

    private static void ThrowExceptionIfCannotFindNameGenerator(IFileSystemBuilder builder)
    {
        if(!builder.Properties.ContainsKey(NameGeneratorName))
            throw new InvalidOperationException("Naming Generator do not added");
    }
    
}