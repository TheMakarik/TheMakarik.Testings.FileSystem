using System;
using System.Collections.Generic;
using System.IO;

namespace TheMakarik.Testing.FileSystem.Core;

public class FileSystemBuilder : IFileSystemBuilder
{
    #region Fields

    private string? _root;
    private HashSet<Action<string, IFileSystemBuilder>> _builderActions = new(capacity: 10);

    #endregion
    
    #region Properties
    
    public string RootDirectory => _root ?? throw
        new InvalidOperationException("Cannot get the root directory because it is not declares, use AddRoot(path) method to declare");
    
    #endregion

    public IFileSystemBuilder AddRoot(string root)
    {
        Guard.AgainstNull(root);
        
        if (this._root is not null)
            throw new InvalidOperationException("Cannot add root directory twice");
        this._root = root;
        
        
        return this;
    }

    public IFileSystemBuilder Add(string rootRelativePath, Action<string, IFileSystemBuilder> additionalAction)
    {
        Guard.AgainstNull(rootRelativePath);
        this._builderActions.Add(additionalAction);
        return this;
    }

    public IFileSystem Build()
    {
        Directory.CreateDirectory(this.RootDirectory);
        try
        {
            foreach (var action in this._builderActions)
                action.Invoke(this.RootDirectory, this);
        }
        catch (Exception e)
        {
            Directory.Delete(this.RootDirectory,  recursive: true);
            throw;
        }
       
        return new FileSystem();
    }
}