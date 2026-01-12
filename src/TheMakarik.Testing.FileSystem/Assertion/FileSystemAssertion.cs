using System;
using TheMakarik.Testing.FileSystem.Exceptions;

namespace TheMakarik.Testing.FileSystem.Assertion;



/// <summary>
/// Implementation of <see cref="IFileSystemAssertion"/> that provides
/// fluent validation methods for file system states in test scenarios.
/// </summary>
/// <remarks>
/// <para>
/// This sealed class is the primary implementation used for asserting
/// file system conditions in integration tests. It works with the
/// <see cref="IFileSystem"/> interface created by the builder pattern
/// to validate file contents, directory structures, and other file system
/// properties.
/// </para>
/// <para>
/// The class follows the Fluent Interface pattern, allowing method chaining
/// for multiple assertions in a single test.
/// </para>
/// <para>
/// All validations throw <see cref="FileSystemAssertionException"/> when
/// the expected condition is not met, providing clear failure messages
/// for test reporting.
/// </para>
/// </remarks>
public sealed class FileSystemAssertion(IFileSystem fileSystem) : IFileSystemAssertion
{
    /// <summary>
    /// Validates a condition against the file system using a custom predicate function.
    /// </summary>
    /// <param name="rootRelativePath">
    /// The relative path from the file system root to validate.
    /// This can be a file or directory path.
    /// </param>
    /// <param name="exceptionMessage">
    /// The message to include in the exception if validation fails.
    /// This should describe what condition was expected.
    /// </param>
    /// <param name="predicate">
    /// A function that takes the root relative path and file system instance,
    /// and returns true if the validation passes, false otherwise.
    /// The first parameter is the rootRelativePath, and the second is the
    /// fileSystem instance against which to validate.
    /// </param>
    /// <returns>
    /// The same <see cref="IFileSystemAssertion"/> instance for method chaining.
    /// </returns>
    /// <exception cref="FileSystemAssertionException">
    /// Thrown when:
    /// <list type="bullet">
    /// <item>The predicate returns false (validation failure)</item>
    /// <item>An exception occurs during predicate execution (wrapped in FileSystemAssertionException)</item>
    /// </list>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is the core validation mechanism that enables custom
    /// assertion logic. It handles exceptions from the predicate by wrapping
    /// them in <see cref="FileSystemAssertionException"/> with an appropriate
    /// message.
    /// </para>
    /// <para>
    /// The method returns the same instance to support fluent chaining,
    /// allowing multiple validations to be performed in sequence.
    /// </para>
    /// <para>
    /// When the predicate throws an exception, it is caught and re-thrown
    /// as an inner exception of <see cref="FileSystemAssertionException"/>
    /// with the message "Inner exception occurred".
    /// </para>
    /// </remarks>
    public IFileSystemAssertion Validate(
        string rootRelativePath, 
        string exceptionMessage, 
        Func<string, IFileSystem, bool> predicate)
    {
        try
        {
            if (predicate(rootRelativePath, fileSystem))
                return this;

        }
        catch (Exception e)
        {
            throw new FileSystemAssertionException("Inner exception", e);
        }
        
        throw new FileSystemAssertionException(exceptionMessage);
    }
}