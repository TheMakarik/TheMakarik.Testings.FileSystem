using System;
using TheMakarik.Testing.FileSystem.Exceptions;

namespace TheMakarik.Testing.FileSystem.Assertion;

/// <summary>
/// Reversed version of the <see cref="FileSystemAssertion"/>
/// </summary>
/// <param name="fileSystem"></param>
internal class FileSystemReversedAssertions(IFileSystem fileSystem) : IFileSystemAssertion
{
    /// <summary>
    /// Reversed version of the <see cref="FileSystemAssertion.Validate"/> that falls if predicate is true
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
    /// and returns false if the validation passes, true otherwise.
    /// The first parameter is the rootRelativePath, and the second is the
    /// fileSystem instance against which to validate.
    /// </param>
    /// <returns>
    /// The same <see cref="IFileSystemAssertion"/> instance for method chaining.
    /// </returns>
    public IFileSystemAssertion Validate(
        string rootRelativePath, 
        string exceptionMessage, 
        Func<string, IFileSystem, bool> predicate)
    {
        try
        {
            if (!predicate(rootRelativePath, fileSystem))
                return this;

        }
        catch (Exception e)
        {
            throw new FileSystemAssertionException("Inner exception", e);
        }
        
        throw new FileSystemAssertionException(exceptionMessage);
    }

    /// <summary>
    /// Reversed <see cref="FileSystemReversedAssertions"/> version (<see cref="FileSystemAssertion"/>
    /// </summary>
    public IFileSystemAssertion No => new FileSystemAssertion(fileSystem);
}