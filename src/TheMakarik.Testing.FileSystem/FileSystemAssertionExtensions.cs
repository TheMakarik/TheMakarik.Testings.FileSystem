using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
/// <see cref="IFileSystem.RootDirectory"/>.
/// </para>
/// <para>
/// These extensions provide convenient ways to validate common file system 
/// conditions without requiring custom predicate functions.
/// </para>
/// </remarks>
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
            (relativePath, system) => File.ReadLines(Path.Combine(system.RootPath, relativePath)).Any());
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
            (relativePath, system) => File.ReadAllText(Path.Combine(system.RootPath, relativePath)) == content
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
    public static IFileSystemAssertion ItemBe(this IFileSystemAssertion assertion, string rootRelativePath, Predicate<string> predicate)
    {
        return assertion.Validate(rootRelativePath, 
            "Custom predicate fails", 
            (path, system) => predicate(Path.Combine(system.RootPath, rootRelativePath)));
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
                var fullPath = Path.Combine(system.RootPath, relativePath);
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
            (relativePath, system) => File.Exists(Path.Combine(system.RootPath, relativePath))
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
            (relativePath, system) => Directory.Exists(Path.Combine(system.RootPath, relativePath))
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
                var fullPath = Path.Combine(system.RootPath, relativePath);
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
    public static IFileSystemAssertion FileHasSize(this IFileSystemAssertion assertion, string rootRelativePath, long expectedSize)
    {
        return assertion.Validate(
            rootRelativePath,
            $"File '{rootRelativePath}' does not have size {expectedSize} bytes",
            (relativePath, system) => 
            {
                var fullPath = Path.Combine(system.RootPath, relativePath);
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
    public static IFileSystemAssertion FileContains(this IFileSystemAssertion assertion, string rootRelativePath, string text, StringComparison stringComparison = StringComparison.Ordinal)
    {
        return assertion.Validate(
            rootRelativePath,
            $"File '{rootRelativePath}' does not contain text: {text}",
            (relativePath, system) => 
            {
                var fullPath = Path.Combine(system.RootPath, relativePath);
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
    public static IFileSystemAssertion FileMatchesRegex(this IFileSystemAssertion assertion, string rootRelativePath, string pattern)
    {
        return assertion.Validate(
            rootRelativePath,
            $"File '{rootRelativePath}' does not match regex pattern: {pattern}",
            (relativePath, system) => 
            {
                var fullPath = Path.Combine(system.RootPath, relativePath);
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
    public static IFileSystemAssertion DirectoryHasFileCount(this IFileSystemAssertion assertion, string rootRelativePath, int expectedCount)
    {
        return assertion.Validate(
            rootRelativePath,
            $"Directory '{rootRelativePath}' does not contain exactly {expectedCount} files",
            (relativePath, system) => 
            {
                var fullPath = Path.Combine(system.RootPath, relativePath);
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
    public static IFileSystemAssertion FileModifiedAfter(this IFileSystemAssertion assertion, string rootRelativePath, DateTime dateTime)
    {
        return assertion.Validate(
            rootRelativePath,
            $"File '{rootRelativePath}' was not modified after {dateTime}",
            (relativePath, system) => 
            {
                var fullPath = Path.Combine(system.RootPath, relativePath);
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
    public static IFileSystemAssertion FileCreatedBefore(this IFileSystemAssertion assertion, string rootRelativePath, DateTime dateTime)
    {
        return assertion.Validate(
            rootRelativePath,
            $"File '{rootRelativePath}' was not created before {dateTime}",
            (relativePath, system) => 
            {
                var fullPath = Path.Combine(system.RootPath, relativePath);
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
    public static IFileSystemAssertion FileHasAttributes(this IFileSystemAssertion assertion, string rootRelativePath, FileAttributes attributes)
    {
        return assertion.Validate(
            rootRelativePath,
            $"File '{rootRelativePath}' does not have attributes: {attributes}",
            (relativePath, system) => 
            {
                var fullPath = Path.Combine(system.RootPath, relativePath);
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
    public static IFileSystemAssertion FileDoesNotHaveAttributes(this IFileSystemAssertion assertion, string rootRelativePath, FileAttributes attributes)
    {
        return assertion.Validate(
            rootRelativePath,
            $"File '{rootRelativePath}' has unexpected attributes: {attributes}",
            (relativePath, system) => 
            {
                var fullPath = Path.Combine(system.RootPath, relativePath);
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
    public static IFileSystemAssertion FilesAreEqual(this IFileSystemAssertion assertion, string firstFilePath, string secondFilePath)
    {
        return assertion.Validate(
            firstFilePath,
            $"Files '{firstFilePath}' and '{secondFilePath}' are not equal",
            (relativePath, system) => 
            {
                var firstFullPath = Path.Combine(system.RootPath, firstFilePath);
                var secondFullPath = Path.Combine(system.RootPath, secondFilePath);
                
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
    public static IFileSystemAssertion FileEndsWith(this IFileSystemAssertion assertion, string rootRelativePath, string endingText, StringComparison stringComparison = StringComparison.Ordinal)
    {
        return assertion.Validate(
            rootRelativePath,
            $"File '{rootRelativePath}' does not end with: {endingText}",
            (relativePath, system) => 
            {
                var fullPath = Path.Combine(system.RootPath, relativePath);
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
    public static IFileSystemAssertion FileStartsWith(this IFileSystemAssertion assertion, string rootRelativePath, string startingText, StringComparison stringComparison = StringComparison.Ordinal)
    {
        return assertion.Validate(
            rootRelativePath,
            $"File '{rootRelativePath}' does not start with: {startingText}",
            (relativePath, system) => 
            {
                var fullPath = Path.Combine(system.RootPath, relativePath);
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
    public static IFileSystemAssertion DirectoryContainsFileWithExtension(this IFileSystemAssertion assertion, string rootRelativePath, string extension)
    {
        return assertion.Validate(
            rootRelativePath,
            $"Directory '{rootRelativePath}' does not contain any files with extension '{extension}'",
            (relativePath, system) => 
            {
                var fullPath = Path.Combine(system.RootPath, relativePath);
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
                var fullPath = Path.Combine(system.RootPath, relativePath);
                var fileContent = File.ReadAllText(fullPath);
                return string.Equals(fileContent, content, comparisonType);
            }
        );
    }
}