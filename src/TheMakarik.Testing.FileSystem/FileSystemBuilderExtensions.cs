using System;
using System.IO;
using JetBrains.Annotations;
using TheMakarik.Testing.FileSystem.Core;

namespace TheMakarik.Testing.FileSystem;

[PublicAPI]
public static class FileSystemBuilderExtensions
{
    public static IFileSystemBuilder AddInTempRoot(this IFileSystemBuilder builder, string root)
    {
        var tempRootFolder = Path.Combine(Path.GetTempPath(), root);
        return builder.AddRoot(tempRootFolder);
    }
    
    public static IFileSystemBuilder AddInTempRoot(this IFileSystemBuilder builder, string root, out string fullPath)
    {
        fullPath = Path.Combine(Path.GetTempPath(), root);
        return builder.AddRoot(fullPath);
    }

    public static IFileSystemBuilder AddRandomRootName(this IFileSystemBuilder builder)
    {
        return builder.AddRoot(Path.GetRandomFileName());
    }
    
    public static IFileSystemBuilder AddRandomRootName(this IFileSystemBuilder builder, out  string rootName)
    {
        rootName = Path.GetRandomFileName();
        return builder.AddRoot(rootName);
    }

    public static IFileSystemBuilder AddFile(this IFileSystemBuilder builder, string rootRelativePath)
    {
        return builder
            .Add(rootRelativePath, (fullPath, _) => File.Create(fullPath).Dispose());
    }
    
    public static IFileSystemBuilder AddFile(this IFileSystemBuilder builder,
        string rootRelativePath, 
        out string rootPath)
    {
        rootPath = Path.Combine(builder.RootDirectory, rootRelativePath);
        return builder
            .Add(rootRelativePath, (fullPath, _) => File.Create(fullPath).Dispose());
    }
    
    public static IFileSystemBuilder AddFile(this IFileSystemBuilder builder, 
        string rootRelativePath,
        string content)
    {
        return builder
            .Add(rootRelativePath, (fullPath, _) =>
            {
                using var stream = File.Create(fullPath);
                using var streamWriter = new StreamWriter(stream);
                streamWriter.Write(content);
            });
    }
    
    public static IFileSystemBuilder AddFile(this IFileSystemBuilder builder,
        string rootRelativePath, 
        string content, 
        out string rootPath)
    {
        rootPath = Path.Combine(builder.RootDirectory, rootRelativePath);
        return builder.AddFile(rootRelativePath, content);
    }

    public static IFileSystemBuilder AddDirectory(this IFileSystemBuilder builder, 
        string rootRelativePath,
        Func<string, IFileSystemBuilder, IFileSystemBuilder>? directoryContentLazyCreational = null)
    {
        return builder.Add(rootRelativePath, (fullPath, _) =>
        {
            if (directoryContentLazyCreational is null)
            {
                Directory.CreateDirectory(fullPath);
                return;
            }
            
            var directoryContentBuilder = FileSystem.BeginBuilding();
            directoryContentBuilder.AddRoot(fullPath);
            directoryContentLazyCreational.Invoke(fullPath, directoryContentBuilder);
            directoryContentBuilder.Build();
        });
    }

    public static IFileSystemBuilder AddDirectory(this IFileSystemBuilder builder, 
        out string rootPath,
        string rootRelativePath,
        Func<string, IFileSystemBuilder, IFileSystemBuilder>? directoryContentLazyCreational = null)
    {
        rootPath = Path.Combine(builder.RootDirectory, rootRelativePath);
        return builder.AddDirectory(rootRelativePath, directoryContentLazyCreational);
    }
}