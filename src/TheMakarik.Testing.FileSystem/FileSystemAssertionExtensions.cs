using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using TheMakarik.Testing.FileSystem.Assertion;

namespace TheMakarik.Testing.FileSystem;

/// <summary>
/// Provides extension methods for <see cref="IFileSystemAssertion"/> to simplify 
/// validation of file system states in test scenarios.
/// </summary>
/// <remarks>
/// <para>
/// This class contains fluent assertion methods that operate exclusively on the 
/// current root directory of the file system. All paths are relative to the 
/// <see cref="IFileSystem.Root"/>.
/// </para>
/// <para>
/// These extensions provide convenient ways to validate common file system 
/// conditions without requiring custom predicate functions.
/// </para>
/// </remarks>
[PublicAPI]
public static class FileSystemAssertionExtensions
{
    /// <summary>
    /// Asserts that a file or directory exists at the specified relative path.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path from the root directory.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method checks for existence of either a file or directory.
    /// If you need to specifically check for a file or directory, use 
    /// <see cref="FileExists"/> or <see cref="DirectoryExists"/> instead.
    /// </remarks>
    public static IFileSystemAssertion Contains(this IFileSystemAssertion assertion, string rootRelativePath)
    {
        return assertion.Validate(
            rootRelativePath,
            $"{rootRelativePath} does not exist in the file system",
            (relativePath, system) => system.Contains(relativePath));
    }

    /// <summary>
    /// Asserts that a file or directory does NOT exist at the specified relative path.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path from the root directory.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    public static IFileSystemAssertion NotContains(this IFileSystemAssertion assertion, string rootRelativePath)
    {
        return assertion.Validate(
            rootRelativePath,
            $"{rootRelativePath} exists in the file system",
            (relativePath, system) => !system.Contains(relativePath));
    }

    /// <summary>
    /// Asserts that a file exists and contains at least one line of content.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the file from the root directory.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method reads the file to check if it has any content. For performance reasons
    /// with large files, it stops reading after the first line.
    /// </remarks>
    public static IFileSystemAssertion BeNotEmptyFile(this IFileSystemAssertion assertion, string rootRelativePath)
    {
        return assertion.Validate(
            rootRelativePath,
            $"{rootRelativePath} does not exist or is empty",
            (relativePath, system) => File.ReadLines(Path.Combine(system.Root, relativePath)).Any());
    }

    /// <summary>
    /// Asserts that a file's content exactly matches the expected string.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the file from the root directory.</param>
    /// <param name="content">The expected file content.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// Comparison is performed using exact string matching. For case-insensitive
    /// or culture-aware comparisons, use <see cref="FileContentEquals(string, string, StringComparison)"/>.
    /// </remarks>
    public static IFileSystemAssertion FileContentEquals(
        this IFileSystemAssertion assertion,
        string rootRelativePath,
        string content)
    {
        return assertion.Validate(
            rootRelativePath,
            $"{rootRelativePath} content does not match expected content: {content}",
            (relativePath, system) => File.ReadAllText(Path.Combine(system.Root, relativePath)) == content
        );
    }

    /// <summary>
    /// Asserts that a file or directory satisfies a custom predicate.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path from the root directory.</param>
    /// <param name="predicate">The custom predicate to evaluate on the full path.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method allows for custom validation logic that isn't covered by
    /// built-in assertion methods. The predicate receives the full path to
    /// the file or directory.
    /// </remarks>
    public static IFileSystemAssertion ItemBe(this IFileSystemAssertion assertion, string rootRelativePath,
        Predicate<string> predicate)
    {
        return assertion.Validate(rootRelativePath,
            "Custom predicate fails",
            (path, system) => predicate(Path.Combine(system.Root, rootRelativePath)));
    }
    
    /// <summary>
    /// Asserts that a root directory executes predicate
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path from the root directory.</param>
    /// <param name="predicate">The custom predicate to evaluate on the full path.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method allows for custom validation logic that isn't covered by
    /// built-in assertion methods. The predicate receives the full path to
    /// the file or directory.
    /// </remarks>
    public static IFileSystemAssertion Be(this IFileSystemAssertion assertion,
        Predicate<string> predicate)
    {
        return assertion.Validate(string.Empty,
            "Custom predicate fails",
            (path, system) => predicate(system.Root));
    }

    /// <summary>
    /// Asserts that a directory is empty (contains no files or subdirectories).
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">
    /// The relative path to the directory from the root directory.
    /// If null, validates the root directory itself.
    /// </param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    public static IFileSystemAssertion HasNoDirectoryContent(this IFileSystemAssertion assertion,
        string? rootRelativePath = null)
    {
        string path = rootRelativePath ?? string.Empty;
        return assertion.Validate(
            path,
            $"Directory '{path}' is not empty",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                return !Directory.EnumerateFileSystemEntries(fullPath).Any();
            }
        );
    }

    /// <summary>
    /// Asserts that a specific file exists (not a directory).
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the file from the root directory.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    public static IFileSystemAssertion FileExists(this IFileSystemAssertion assertion, string rootRelativePath)
    {
        return assertion.Validate(
            rootRelativePath,
            $"File '{rootRelativePath}' does not exist",
            (relativePath, system) => File.Exists(Path.Combine(system.Root, relativePath))
        );
    }

    /// <summary>
    /// Asserts that a specific directory exists (not a file).
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the directory from the root directory.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    public static IFileSystemAssertion DirectoryExists(this IFileSystemAssertion assertion, string rootRelativePath)
    {
        return assertion.Validate(
            rootRelativePath,
            $"Directory '{rootRelativePath}' does not exist",
            (relativePath, system) => Directory.Exists(Path.Combine(system.Root, relativePath))
        );
    }

    /// <summary>
    /// Asserts that a file is empty (0 bytes in size).
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the file from the root directory.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    public static IFileSystemAssertion FileIsEmpty(this IFileSystemAssertion assertion, string rootRelativePath)
    {
        return assertion.Validate(
            rootRelativePath,
            $"File '{rootRelativePath}' is not empty",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                return new FileInfo(fullPath).Length == 0;
            }
        );
    }

    /// <summary>
    /// Asserts that a file has exactly the specified size in bytes.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the file from the root directory.</param>
    /// <param name="expectedSize">The expected file size in bytes.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    public static IFileSystemAssertion FileHasSize(this IFileSystemAssertion assertion, string rootRelativePath,
        long expectedSize)
    {
        return assertion.Validate(
            rootRelativePath,
            $"File '{rootRelativePath}' does not have size {expectedSize} bytes",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                return new FileInfo(fullPath).Length == expectedSize;
            }
        );
    }

    /// <summary>
    /// Asserts that a file contains the specified text.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the file from the root directory.</param>
    /// <param name="text">The text that should be present in the file.</param>
    /// <param name="stringComparison">The string comparison rules to use. Default is <see cref="StringComparison.Ordinal"/>.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    public static IFileSystemAssertion FileContains(this IFileSystemAssertion assertion, string rootRelativePath,
        string text, StringComparison stringComparison = StringComparison.Ordinal)
    {
        return assertion.Validate(
            rootRelativePath,
            $"File '{rootRelativePath}' does not contain text: {text}",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                var content = File.ReadAllText(fullPath);
                return content.IndexOf(text, stringComparison) >= 0;
            }
        );
    }

    /// <summary>
    /// Asserts that a file's content matches a regular expression pattern.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the file from the root directory.</param>
    /// <param name="pattern">The regular expression pattern to match.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    public static IFileSystemAssertion FileMatchesRegex(this IFileSystemAssertion assertion, string rootRelativePath,
        string pattern)
    {
        return assertion.Validate(
            rootRelativePath,
            $"File '{rootRelativePath}' does not match regex pattern: {pattern}",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                var content = File.ReadAllText(fullPath);
                return Regex.IsMatch(content, pattern);
            }
        );
    }

    /// <summary>
    /// Asserts that a directory contains exactly the specified number of files.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the directory from the root directory.</param>
    /// <param name="expectedCount">The expected number of files in the directory.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    public static IFileSystemAssertion DirectoryHasFileCount(this IFileSystemAssertion assertion,
        string rootRelativePath, int expectedCount)
    {
        return assertion.Validate(
            rootRelativePath,
            $"Directory '{rootRelativePath}' does not contain exactly {expectedCount} files",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                return Directory.GetFiles(fullPath).Length == expectedCount;
            }
        );
    }

    /// <summary>
    /// Asserts that a file was last modified after the specified date and time.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the file from the root directory.</param>
    /// <param name="dateTime">The date and time to compare against.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    public static IFileSystemAssertion FileModifiedAfter(this IFileSystemAssertion assertion, string rootRelativePath,
        DateTime dateTime)
    {
        return assertion.Validate(
            rootRelativePath,
            $"File '{rootRelativePath}' was not modified after {dateTime}",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                return File.GetLastWriteTime(fullPath) > dateTime;
            }
        );
    }

    /// <summary>
    /// Asserts that a file was created before the specified date and time.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the file from the root directory.</param>
    /// <param name="dateTime">The date and time to compare against.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    public static IFileSystemAssertion FileCreatedBefore(this IFileSystemAssertion assertion, string rootRelativePath,
        DateTime dateTime)
    {
        return assertion.Validate(
            rootRelativePath,
            $"File '{rootRelativePath}' was not created before {dateTime}",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                return File.GetCreationTime(fullPath) < dateTime;
            }
        );
    }

    /// <summary>
    /// Asserts that a file has the specified attributes.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the file from the root directory.</param>
    /// <param name="attributes">The file attributes to check for.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    public static IFileSystemAssertion FileHasAttributes(this IFileSystemAssertion assertion, string rootRelativePath,
        FileAttributes attributes)
    {
        return assertion.Validate(
            rootRelativePath,
            $"File '{rootRelativePath}' does not have attributes: {attributes}",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                return (File.GetAttributes(fullPath) & attributes) == attributes;
            }
        );
    }

    /// <summary>
    /// Asserts that a file does NOT have the specified attributes.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the file from the root directory.</param>
    /// <param name="attributes">The file attributes that should NOT be present.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    public static IFileSystemAssertion FileDoesNotHaveAttributes(this IFileSystemAssertion assertion,
        string rootRelativePath, FileAttributes attributes)
    {
        return assertion.Validate(
            rootRelativePath,
            $"File '{rootRelativePath}' has unexpected attributes: {attributes}",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                return (File.GetAttributes(fullPath) & attributes) == 0;
            }
        );
    }

    /// <summary>
    /// Asserts that two files have identical content.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="firstFilePath">The relative path to the first file from the root directory.</param>
    /// <param name="secondFilePath">The relative path to the second file from the root directory.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    public static IFileSystemAssertion FilesAreEqual(this IFileSystemAssertion assertion, string firstFilePath,
        string secondFilePath)
    {
        return assertion.Validate(
            firstFilePath,
            $"Files '{firstFilePath}' and '{secondFilePath}' are not equal",
            (relativePath, system) =>
            {
                var firstFullPath = Path.Combine(system.Root, firstFilePath);
                var secondFullPath = Path.Combine(system.Root, secondFilePath);

                var firstBytes = File.ReadAllBytes(firstFullPath);
                var secondBytes = File.ReadAllBytes(secondFullPath);

                return firstBytes.SequenceEqual(secondBytes);
            }
        );
    }

    /// <summary>
    /// Asserts that a file ends with the specified text.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the file from the root directory.</param>
    /// <param name="endingText">The text that should appear at the end of the file.</param>
    /// <param name="stringComparison">The string comparison rules to use. Default is <see cref="StringComparison.Ordinal"/>.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    public static IFileSystemAssertion FileEndsWith(this IFileSystemAssertion assertion, string rootRelativePath,
        string endingText, StringComparison stringComparison = StringComparison.Ordinal)
    {
        return assertion.Validate(
            rootRelativePath,
            $"File '{rootRelativePath}' does not end with: {endingText}",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                var content = File.ReadAllText(fullPath);
                return content.EndsWith(endingText, stringComparison);
            }
        );
    }

    /// <summary>
    /// Asserts that a file starts with the specified text.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the file from the root directory.</param>
    /// <param name="startingText">The text that should appear at the beginning of the file.</param>
    /// <param name="stringComparison">The string comparison rules to use. Default is <see cref="StringComparison.Ordinal"/>.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    public static IFileSystemAssertion FileStartsWith(this IFileSystemAssertion assertion, string rootRelativePath,
        string startingText, StringComparison stringComparison = StringComparison.Ordinal)
    {
        return assertion.Validate(
            rootRelativePath,
            $"File '{rootRelativePath}' does not start with: {startingText}",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                var content = File.ReadAllText(fullPath);
                return content.StartsWith(startingText, stringComparison);
            }
        );
    }

    /// <summary>
    /// Asserts that a directory contains at least one file with the specified extension.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the directory from the root directory.</param>
    /// <param name="extension">The file extension to search for (including the dot, e.g., ".txt").</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    public static IFileSystemAssertion DirectoryContainsFileWithExtension(this IFileSystemAssertion assertion,
        string rootRelativePath, string extension)
    {
        return assertion.Validate(
            rootRelativePath,
            $"Directory '{rootRelativePath}' does not contain any files with extension '{extension}'",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                return Directory.GetFiles(fullPath, $"*{extension}").Any();
            }
        );
    }

    /// <summary>
    /// Asserts that a file's content equals another string using the specified string comparison rules.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the file from the root directory.</param>
    /// <param name="content">The expected file content.</param>
    /// <param name="comparisonType">The string comparison rules to use.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    public static IFileSystemAssertion FileContentEquals(
        this IFileSystemAssertion assertion,
        string rootRelativePath,
        string content,
        StringComparison comparisonType)
    {
        return assertion.Validate(
            rootRelativePath,
            $"{rootRelativePath} content does not match expected content: {content}",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                var fileContent = File.ReadAllText(fullPath);
                return string.Equals(fileContent, content, comparisonType);
            }
        );
    }

    /// <summary>
    /// Asserts that a directory contains a specific file by name.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the directory from the root directory.</param>
    /// <param name="fileName">The name of the file to search for (without path).</param>
    /// <param name="searchOption">Specifies whether to search in subdirectories. Default is <see cref="SearchOption.TopDirectoryOnly"/>.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method searches for a file by name within the specified directory.
    /// Use this when you need to verify that a specific file exists in a directory,
    /// regardless of its exact relative path from the current directory.
    /// </remarks>
    public static IFileSystemAssertion DirectoryContainsFile(
        this IFileSystemAssertion assertion,
        string rootRelativePath,
        string fileName,
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        return assertion.Validate(
            rootRelativePath,
            $"Directory '{rootRelativePath}' does not contain file '{fileName}'",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                return Directory.GetFiles(fullPath, fileName, searchOption).Any();
            }
        );
    }

    /// <summary>
    /// Asserts that a directory contains a specific subdirectory by name.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the directory from the root directory.</param>
    /// <param name="directoryName">The name of the subdirectory to search for (without path).</param>
    /// <param name="searchOption">Specifies whether to search in nested subdirectories. Default is <see cref="SearchOption.TopDirectoryOnly"/>.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method searches for a subdirectory by name within the specified directory.
    /// Use this when you need to verify that a specific subdirectory exists in a directory,
    /// regardless of its exact relative path from the current directory.
    /// </remarks>
    public static IFileSystemAssertion DirectoryContainsSubdirectory(
        this IFileSystemAssertion assertion,
        string rootRelativePath,
        string directoryName,
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        return assertion.Validate(
            rootRelativePath,
            $"Directory '{rootRelativePath}' does not contain subdirectory '{directoryName}'",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                return Directory.GetDirectories(fullPath, directoryName, searchOption).Any();
            }
        );
    }

    /// <summary>
    /// Asserts that a directory contains all files from the specified list.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the directory from the root directory.</param>
    /// <param name="fileNames">Array of file names to search for (without path).</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method verifies that a directory contains all the specified files.
    /// It only checks for existence and does not verify file contents.
    /// If any of the specified files are missing, the assertion fails.
    /// </remarks>
    public static IFileSystemAssertion DirectoryContainsAllFiles(
        this IFileSystemAssertion assertion,
        string rootRelativePath,
        params string[] fileNames)
    {
        return assertion.Validate(
            rootRelativePath,
            $"Directory '{rootRelativePath}' does not contain all specified files: {string.Join(", ", fileNames)}",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                var existingFiles = Directory.GetFiles(fullPath).Select(Path.GetFileName);
                return fileNames.All(fileName => existingFiles.Contains(fileName));
            }
        );
    }

    /// <summary>
    /// Asserts that a directory contains exactly the specified files (no extra files).
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the directory from the root directory.</param>
    /// <param name="fileNames">Array of expected file names (without path).</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method verifies that a directory contains exactly the specified files and no others.
    /// The order of files is not considered. Subdirectories are ignored in this comparison.
    /// Use this method when you need to ensure a directory has a precise set of files.
    /// </remarks>
    public static IFileSystemAssertion DirectoryContainsExactlyFiles(
        this IFileSystemAssertion assertion,
        string rootRelativePath,
        params string[] fileNames)
    {
        return assertion.Validate(
            rootRelativePath,
            $"Directory '{rootRelativePath}' does not contain exactly the specified files: {string.Join(", ", fileNames)}",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                var existingFiles = Directory.GetFiles(fullPath).Select(Path.GetFileName).ToList();
                var expectedFiles = fileNames.ToList();

                return existingFiles.Count == expectedFiles.Count &&
                       existingFiles.All(file => expectedFiles.Contains(file));
            }
        );
    }

    /// <summary>
    /// Asserts that all files in a directory satisfy a specific predicate.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the directory from the root directory.</param>
    /// <param name="predicate">The predicate to evaluate on each file name in the directory.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method applies the specified predicate to all files in the directory.
    /// The predicate receives only the file name (without path) for each file.
    /// The assertion fails if any file does not satisfy the predicate.
    /// </remarks>
    public static IFileSystemAssertion AllFilesInDirectorySatisfy(
        this IFileSystemAssertion assertion,
        string rootRelativePath,
        Predicate<string> predicate)
    {
        return assertion.Validate(
            rootRelativePath,
            $"Not all files in directory '{rootRelativePath}' satisfy the predicate",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                var files = Directory.GetFiles(fullPath).Select(Path.GetFileName);
                return files.All(fileName => predicate(fileName));
            }
        );
    }

    /// <summary>
    /// Asserts that a directory contains at least one file that satisfies a specific predicate.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the directory from the root directory.</param>
    /// <param name="predicate">The predicate to evaluate on each file name in the directory.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method checks if any file in the directory satisfies the specified predicate.
    /// The predicate receives only the file name (without path) for each file.
    /// The assertion fails if no file satisfies the predicate.
    /// </remarks>
    public static IFileSystemAssertion DirectoryContainsFileSatisfying(
        this IFileSystemAssertion assertion,
        string rootRelativePath,
        Predicate<string> predicate)
    {
        return assertion.Validate(
            rootRelativePath,
            $"Directory '{rootRelativePath}' does not contain any file satisfying the predicate",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                var files = Directory.GetFiles(fullPath).Select(Path.GetFileName);
                return files.Any(fileName => predicate(fileName));
            }
        );
    }

    /// <summary>
    /// Asserts that a directory contains a specific number of subdirectories.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the directory from the root directory.</param>
    /// <param name="expectedCount">The expected number of subdirectories in the directory.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method counts only direct subdirectories (not nested ones).
    /// Use this when you need to verify the exact number of child directories.
    /// </remarks>
    public static IFileSystemAssertion DirectoryHasSubdirectoryCount(
        this IFileSystemAssertion assertion,
        string rootRelativePath,
        int expectedCount)
    {
        return assertion.Validate(
            rootRelativePath,
            $"Directory '{rootRelativePath}' does not contain exactly {expectedCount} subdirectories",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                return Directory.GetDirectories(fullPath).Length == expectedCount;
            }
        );
    }

    /// <summary>
    /// Asserts that a directory contains files with specific extensions.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="rootRelativePath">The relative path to the directory from the root directory.</param>
    /// <param name="extensions">Array of file extensions to check for (with or without leading dot).</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method verifies that all files in the directory have one of the specified extensions.
    /// Extensions can be provided with or without a leading dot (e.g., ".txt" or "txt").
    /// The assertion fails if any file has a different extension.
    /// </remarks>
    public static IFileSystemAssertion AllFilesHaveExtensions(
        this IFileSystemAssertion assertion,
        string rootRelativePath,
        params string[] extensions)
    {
        return assertion.Validate(
            rootRelativePath,
            $"Not all files in directory '{rootRelativePath}' have the specified extensions: {string.Join(", ", extensions)}",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                var files = Directory.GetFiles(fullPath);

                // Normalize extensions: ensure they start with a dot
                var normalizedExtensions = extensions.Select(ext =>
                    ext.StartsWith(".") ? ext : $".{ext}").ToHashSet();

                return files.All(file =>
                {
                    var fileExtension = Path.GetExtension(file);
                    return normalizedExtensions.Contains(fileExtension);
                });
            }
        );
    }
    
        /// <summary>
    /// Asserts that the root directory contains exactly the specified files and directories.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="expectedItems">The collection of expected file and directory names.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method validates that the root directory contains exactly the specified items
    /// (both files and directories) and no additional items. The comparison is performed
    /// using case-sensitive file name matching.
    /// </remarks>
    public static IFileSystemAssertion ContentEquals(
        this IFileSystemAssertion assertion,
        IEnumerable<string> expectedItems)
    {
        return assertion.Validate(
            string.Empty,
            $"Root directory content does not match expected items: {string.Join(", ", expectedItems)}",
            (relativePath, system) =>
            {
                var actualItems = Directory.GetFileSystemEntries(system.Root);
                var expectedList = expectedItems.ToList();

                return actualItems.All(item => expectedList.Contains(item));
            }
        );
    }

    /// <summary>
    /// Asserts that the root directory contains only files (no subdirectories).
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method verifies that the root directory contains no subdirectories.
    /// It does not check the number or names of files, only ensures there are no directories.
    /// </remarks>
    public static IFileSystemAssertion ContentContainsOnlyFiles(
        this IFileSystemAssertion assertion)
    {
        return assertion.Validate(
            string.Empty,
            "Root directory contains subdirectories",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                return !Directory.GetDirectories(fullPath).Any();
            }
        );
    }

    /// <summary>
    /// Asserts that the root directory contains only directories (no files).
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method verifies that the root directory contains no files.
    /// It does not check the number or names of directories, only ensures there are no files.
    /// </remarks>
    public static IFileSystemAssertion ContentContainsOnlyDirectories(
        this IFileSystemAssertion assertion)
    {
        return assertion.Validate(
            string.Empty,
            "Root directory contains files",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                return !Directory.GetFiles(fullPath).Any();
            }
        );
    }

    /// <summary>
    /// Asserts that the root directory has a total size less than the specified limit.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="maxSizeInBytes">The maximum allowed total size in bytes.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method calculates the total size of all files in the root directory
    /// (including files in subdirectories) and asserts it's less than the specified limit.
    /// Directories themselves do not contribute to the size calculation.
    /// </remarks>
    public static IFileSystemAssertion TotalSizeLessThan(
        this IFileSystemAssertion assertion,
        long maxSizeInBytes)
    {
        return assertion.Validate(
            string.Empty,
            $"Total size of root directory exceeds {maxSizeInBytes} bytes",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                var allFiles = Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories);
                var totalSize = allFiles.Sum(file => new FileInfo(file).Length);
                return totalSize < maxSizeInBytes;
            }
        );
    }

    /// <summary>
    /// Asserts that all files in the root directory have names matching the specified pattern.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="namePattern">The regex pattern to match file names against.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method validates that all files (not directories) in the root directory
    /// have names that match the specified regular expression pattern.
    /// The pattern is applied to file names only (without path or extension).
    /// </remarks>
    public static IFileSystemAssertion AllFilesMatchNamePattern(
        this IFileSystemAssertion assertion,
        string namePattern)
    {
        return assertion.Validate(
            string.Empty,
            $"Not all files in root directory match name pattern: {namePattern}",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                var files = Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories);
                return files.All(file =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    return Regex.IsMatch(fileName, namePattern);
                });
            }
        );
    }

    /// <summary>
    /// Asserts that the root directory has a specific file count (including files in subdirectories).
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="expectedFileCount">The expected total number of files.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method counts all files in the root directory recursively.
    /// It includes files in all subdirectories but excludes directory entries themselves.
    /// </remarks>
    public static IFileSystemAssertion TotalFileCount(
        this IFileSystemAssertion assertion,
        int expectedFileCount)
    {
        return assertion.Validate(
            string.Empty,
            $"Root directory does not contain exactly {expectedFileCount} files",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                var allFiles = Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories);
                return allFiles.Length == expectedFileCount;
            }
        );
    }

    /// <summary>
    /// Asserts that the root directory has no duplicate file names.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method checks that all files in the root directory have unique names.
    /// The comparison is case-sensitive. Files in different subdirectories with
    /// the same name are considered duplicates.
    /// </remarks>
    public static IFileSystemAssertion NoDuplicateFileNames(
        this IFileSystemAssertion assertion)
    {
        return assertion.Validate(
            string.Empty,
            "Root directory contains files with duplicate names",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                var allFiles = Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories);
                var fileNames = allFiles.Select(Path.GetFileName);
                return fileNames.Distinct().Count() == fileNames.Count();
            }
        );
    }

    /// <summary>
    /// Asserts that the root directory contains at least one file with content matching the specified text.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="searchText">The text to search for in file contents.</param>
    /// <param name="stringComparison">The string comparison rules to use. Default is <see cref="StringComparison.Ordinal"/>.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method searches through all files in the root directory (including subdirectories)
    /// for the specified text. It stops searching after finding the first match.
    /// Use this for verifying that specific content exists somewhere in the file system.
    /// </remarks>
    public static IFileSystemAssertion ContainsFileWithText(
        this IFileSystemAssertion assertion,
        string searchText,
        StringComparison stringComparison = StringComparison.Ordinal)
    {
        return assertion.Validate(
            string.Empty,
            $"Root directory does not contain any file with text: {searchText}",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                var allFiles = Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories);
                
                foreach (var file in allFiles)
                {
                    try
                    {
                        var content = File.ReadAllText(file);
                        if (content.IndexOf(searchText, stringComparison) >= 0)
                            return true;
                    }
                    catch (IOException)
                    {
                        // Skip files that can't be read
                        continue;
                    }
                }
                
                return false;
            }
        );
    }

    /// <summary>
    /// Asserts that all directories in the root directory have a maximum nesting depth.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="maxDepth">The maximum allowed nesting depth.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method checks that no directory in the root directory tree exceeds
    /// the specified nesting depth. Depth is measured from the root directory
    /// (root has depth 0, its subdirectories have depth 1, etc.).
    /// </remarks>
    public static IFileSystemAssertion MaxDirectoryDepth(
        this IFileSystemAssertion assertion,
        int maxDepth)
    {
        return assertion.Validate(
            string.Empty,
            $"Root directory contains directories deeper than {maxDepth} levels",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                
                bool CheckDirectoryDepth(string directory, int currentDepth)
                {
                    if (currentDepth > maxDepth)
                        return false;
                    
                    foreach (var subDir in Directory.GetDirectories(directory))
                    {
                        if (!CheckDirectoryDepth(subDir, currentDepth + 1))
                            return false;
                    }
                    
                    return true;
                }
                
                return CheckDirectoryDepth(fullPath, 0);
            }
        );
    }

    /// <summary>
    /// Asserts that the root directory follows a specific naming convention for all items.
    /// </summary>
    /// <param name="assertion">The <see cref="IFileSystemAssertion"/> instance.</param>
    /// <param name="conventionValidator">A function that validates item names according to the convention.</param>
    /// <returns>The same <see cref="IFileSystemAssertion"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method validates that all files and directories in the root directory
    /// follow a specific naming convention. The validator function receives each
    /// item name and should return true if the name follows the convention.
    /// </remarks>
    public static IFileSystemAssertion FollowsNamingConvention(
        this IFileSystemAssertion assertion,
        Func<string, bool> conventionValidator)
    {
        return assertion.Validate(
            string.Empty,
            "Root directory contains items that violate the naming convention",
            (relativePath, system) =>
            {
                var fullPath = Path.Combine(system.Root, relativePath);
                var allItems = Directory.GetFileSystemEntries(fullPath)
                    .Select(Path.GetFileName);
                return allItems.All(conventionValidator);
            }
        );
    }
}