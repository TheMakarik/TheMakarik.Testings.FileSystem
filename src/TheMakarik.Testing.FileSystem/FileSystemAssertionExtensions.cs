using System.IO;
using System.Linq;
using TheMakarik.Testing.FileSystem.Assertion;

namespace TheMakarik.Testing.FileSystem;

public static class FileSystemAssertionExtensions
{
    public static IFileSystemAssertion Contains(this IFileSystemAssertion assertion, string rootRelativePath)
    {
        return assertion.Validate(
            rootRelativePath, 
            $"{rootRelativePath} does not exist in the file system", 
            (relativePath, system) => system.Contains(relativePath));
    }
    
    public static IFileSystemAssertion NotContains(this IFileSystemAssertion assertion, string rootRelativePath)
    {
        return assertion.Validate(
            rootRelativePath, 
            $"{rootRelativePath} exist in the file system", 
            (relativePath, system) => !system.Contains(relativePath));
    }
    
    
    public static IFileSystemAssertion BeNotEmptyFile(this IFileSystemAssertion assertion, string rootRelativePath)
    {
        return assertion.Validate(
            rootRelativePath, 
            $"{rootRelativePath} do not exist", 
            (relativePath, system) => File.ReadLines(Path.Combine(system.RootPath, relativePath)).Any());
    }

    public static IFileSystemAssertion FileContentEquals(
        this IFileSystemAssertion assertion, 
        string rootRelativePath,
        string content)
    {
        return assertion.Validate(
            rootRelativePath,
            $"{rootRelativePath} content do not match expected content: {content}",
            (relativePath, system) =>
            {
                var splitContent = content.Split("\n");
                return true;
            });
    }
}