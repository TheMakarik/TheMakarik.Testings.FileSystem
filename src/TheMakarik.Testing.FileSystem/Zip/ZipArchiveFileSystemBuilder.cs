using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace TheMakarik.Testing.FileSystem.Zip;

/// <summary>
/// Represents a builder for creating and configuring zip archives with multiple entries.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="ZipArchiveFileSystemBuilder"/> provides a fluent interface for creating zip archives
/// with multiple files and directories. It supports two modes of operation: creating a new zip archive
/// file or adding entries to an existing <see cref="ZipArchive"/> instance.
/// </para>
/// <para>
/// All entries are created with deferred execution, meaning they are only written to the archive
/// when the <see cref="Build"/> method is called.
/// </para>
/// </remarks>
public sealed class ZipArchiveFileSystemBuilder : IZipArchiveFileSystemBuilder
{
    #region Fields
    
    private readonly ZipArchive? _archive;
    private readonly Dictionary<string, Action<ZipCreationalContext>> _builderActions= new(capacity: 10);
    
    #endregion
    
    #region Constructors
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ZipArchiveFileSystemBuilder"/> class for creating a new zip archive file.
    /// </summary>
    /// <param name="root">The full path to the zip archive file that will be created.</param>
    /// <param name="prefix">The directory prefix within the zip archive. Default is an empty string.</param>
    /// <remarks>
    /// This constructor creates a builder that will create a new zip archive file at the specified path.
    /// The archive file must have a <c>.zip</c> extension.
    /// </remarks>
    public ZipArchiveFileSystemBuilder(string root, string? prefix = null)
    {
        Root = root;
        Prefix = prefix ?? string.Empty;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ZipArchiveFileSystemBuilder"/> class for adding entries to an existing zip archive.
    /// </summary>
    /// <param name="root">The identifier or path associated with the zip archive.</param>
    /// <param name="archive">The existing <see cref="ZipArchive"/> instance to add entries to.</param>
    /// <param name="prefix">The directory prefix within the zip archive. Default is an empty string.</param>
    /// <remarks>
    /// This constructor creates a builder that adds entries to an existing <see cref="ZipArchive"/> instance.
    /// The builder does not own the archive and will not dispose it.
    /// </remarks>
    public ZipArchiveFileSystemBuilder(string root, ZipArchive archive, string? prefix = null)
    {
        this.Root = root;
        this.Prefix = prefix ?? string.Empty;
        this._archive =  archive;
    }
    
    #endregion
    
    #region IZipArchiveFileSystemBuilder implementation

    /// <summary>
    /// Gets the full path to the zip archive file that will be created.
    /// </summary>
    /// <value>
    /// The absolute path to the zip archive file.
    /// </value>
    /// <remarks>
    /// When using the constructor with an existing <see cref="ZipArchive"/>, this property may
    /// represent an identifier rather than a physical file path.
    /// </remarks>
    public string Root { get; }
    
    /// <summary>
    /// Gets the current directory prefix within the zip archive.
    /// All entry names will be prefixed with this path when added to the archive.
    /// </summary>
    /// <value>
    /// The directory prefix path within the zip archive.
    /// </value>
    public string Prefix { get; }
    
    /// <inheritdoc/>
    public IZipArchiveFileSystemBuilder Add(string relativePath, Action<ZipCreationalContext> additionalAction)
    {
       _builderActions.Add(relativePath, additionalAction);   
       return this;
    }

    /// <inheritdoc/>
    public void Build()
    {
        Guard.AgainstNull(Root, "root");
        Debug.Assert(Path.GetExtension(Root) == ".zip");
        
        try
        {
            if (_archive is null)
            {
                using var zipStream = File.Create(Root);
                using var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create);
                ExecuteActions(zipArchive);
            }
            else
            {
                ExecuteActions(_archive);
            }
        }
        catch
        {
            CleanupOnFailure();
            throw;
        }
    }

    private void ExecuteActions(ZipArchive archive)
    {
        foreach (var action in _builderActions)
            action.Value(new ZipCreationalContext(action.Key, archive, Prefix));
        
    }

    private void CleanupOnFailure()
    {
        if (File.Exists(Root))
            File.Delete(Root);
        
    }
    
    #endregion
}