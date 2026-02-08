using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using SharpCompress.Compressors.BZip2;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Writers;
using SharpCompress.Writers.Tar;
using CompressionLevel = SharpCompress.Compressors.Deflate.CompressionLevel;

namespace TheMakarik.Testing.FileSystem.SharpCompress.Tar;

/// <summary>
/// Implementation of <see cref="ITarFileSystemBuilder"/> for creating tar archives,
/// optionally compressed with GZip or BZip2.
/// </summary>
/// <remarks>
/// <para>
/// Entries are added lazily and written only when <see cref="Build"/> is called.
/// </para>
/// <para>
/// Compression level applies only to GZip. BZip2 does not support levels.
/// </para>
/// </remarks>
public class TarFileSystemBuilder : ITarFileSystemBuilder
{
    private readonly Dictionary<string, Action<TarCreationalContext>> _builderActions = new(capacity: 10);
    private TarPackTo _packTo;
    private IWriter? _writer;
    private int _compressionLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="TarFileSystemBuilder"/> class.
    /// </summary>
    /// <param name="root">The full path where the tar file will be created.</param>
    /// <param name="packTo">The compression type to apply.</param>
    /// <param name="compressionLevel">Optional compression level (1-9) for GZip. Ignored for other types.</param>
    /// <param name="prefix">Optional directory prefix for entries.</param>
    /// <exception cref="ArgumentException">Thrown if root does not end with appropriate extension based on packTo.</exception>
    public TarFileSystemBuilder(string root, TarPackTo packTo = TarPackTo.None, int? compressionLevel = null, string? prefix = null)
    {
        Root = root;
        Prefix = prefix ?? string.Empty;
        _packTo = packTo;
        _compressionLevel = compressionLevel ?? (int)CompressionLevel.Default;
        
        Root = NormalizeName(root, packTo); 
    }

    public TarFileSystemBuilder(string root, string prefix, IWriter writer)
    {
        _writer = writer;
        Root = root;
        Prefix = prefix;
    }

    /// <inheritdoc />
    public string Root { get; }

    /// <inheritdoc />
    public Dictionary<object, object> Properties { get; } = new();

    /// <inheritdoc />
    public string Prefix { get; }

    /// <inheritdoc />
    public ITarFileSystemBuilder Add(string relativePath, Action<TarCreationalContext> additionalAction)
    {
        _builderActions.Add(relativePath, additionalAction);
        return this;
    }

    /// <inheritdoc />
    /// <inheritdoc />
    public void Build()
    {
        try
        {
            if (_writer is not null)
            {
                foreach (var keyValuePair in _builderActions)
                    keyValuePair.Value.Invoke(new TarCreationalContext(keyValuePair.Key, _writer, Prefix));
                return;
            }

            using var fileStream = File.Create(Root);
            
            using var writer = WriterFactory.Open(fileStream, ArchiveType.Tar, GetCompressionType());
            foreach (var keyValuePair in _builderActions)
            {
                var context = new TarCreationalContext(keyValuePair.Key, writer, Prefix);
                keyValuePair.Value.Invoke(context);
            }
        }
        catch
        {
            if (File.Exists(Root))
                File.Delete(Root);
            throw;
        }
    }

    private CompressionType GetCompressionType()
    {
        return _packTo switch
        {
            TarPackTo.None => CompressionType.None,
            TarPackTo.GZip => CompressionType.GZip,
            TarPackTo.BZip2 => CompressionType.BZip2,
            _ => CompressionType.None
        };
    }

    private static string NormalizeName(string name, TarPackTo tarPackTo)
    {
        return tarPackTo switch
        {
            TarPackTo.None when Path.GetExtension(name) == ".tar" => name,
            TarPackTo.None => name + ".tar",
            TarPackTo.GZip when name.EndsWith(".tar.gz") => name,
            TarPackTo.GZip when Path.GetExtension(name) == ".tar" => name + ".gz",
            TarPackTo.GZip => name + ".tar.gz",
            TarPackTo.BZip2 when name.EndsWith(".tar.bz2") => name,
            TarPackTo.BZip2 when Path.GetExtension(name) == ".tar" => name + ".bz2",
            TarPackTo.BZip2 => name + ".tar.bz2",
            _ => throw new ArgumentOutOfRangeException(nameof(tarPackTo), tarPackTo, null)
        };
    }
}