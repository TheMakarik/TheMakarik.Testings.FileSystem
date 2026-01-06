using System;

namespace TheMakarik.Testing.FileSystem.Assertion;

public interface IFileSystemAssertion
{
    public IFileSystemAssertion Validate(string rootRelativePath, string exceptionMessage, Func<string, IFileSystem, bool> predicate);
}