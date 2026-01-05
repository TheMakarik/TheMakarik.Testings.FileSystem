using System;

namespace TheMakarik.Testing.FileSystem.Core;

public interface IFileSystemBuilder
{
    public string RootDirectory { get; }
    IFileSystemBuilder AddRoot(string root);
    IFileSystemBuilder Add(string rootRelativePath, Action<string, IFileSystemBuilder> additionalAction);
    IFileSystem Build();
   

}