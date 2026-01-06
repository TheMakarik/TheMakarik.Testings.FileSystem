using System;
using TheMakarik.Testing.FileSystem.Exceptions;

namespace TheMakarik.Testing.FileSystem.Assertion;

public sealed class FileSystemAssertion(IFileSystem fileSystem) : IFileSystemAssertion
{

    public IFileSystemAssertion Validate(string rootRelativePath, string exceptionMessage, Func<string, IFileSystem, bool> predicate)
    {
        try
        {
            return predicate(rootRelativePath, fileSystem) ? this : throw new FileSystemAssertionException(exceptionMessage);
        }
        catch (Exception e)
        {
          throw new FileSystemAssertionException("Inner exception occured", e);
        }
    
    }
}