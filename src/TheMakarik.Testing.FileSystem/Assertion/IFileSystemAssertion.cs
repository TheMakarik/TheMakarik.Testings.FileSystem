using System;
using TheMakarik.Testing.FileSystem.Exceptions;

namespace TheMakarik.Testing.FileSystem.Assertion;

/// <summary>
/// Provides assertion methods for validating file system states and content.
/// This interface defines a contract for validating file system structures, 
/// file contents, and directory states in test scenarios.
/// </summary>
/// <remarks>
/// Implementations of this interface provide a fluent API for performing 
/// various validations on file systems created for integration testing.
/// The validation is typically used in unit test assertions to verify 
/// that file operations produce expected results.
/// </remarks>
public interface IFileSystemAssertion
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
    /// </param>
    /// <returns>
    /// The same <see cref="IFileSystemAssertion"/> instance for method chaining.
    /// </returns>
    /// <exception cref="FileSystemAssertionException">
    /// Thrown when the predicate returns false, indicating validation failure.
    /// The exception includes the provided exception message.
    /// </exception>
    /// <remarks>
    /// This method provides a flexible way to create custom assertions
    /// by allowing test code to supply any validation logic needed.
    /// Use this method when built-in assertion methods don't cover
    /// your specific validation requirements.
    /// </remarks>
    public IFileSystemAssertion Validate(
        string rootRelativePath, 
        string exceptionMessage, 
        Func<string, IFileSystem, bool> predicate);
}