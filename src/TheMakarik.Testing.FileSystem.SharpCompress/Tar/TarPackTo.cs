namespace TheMakarik.Testing.FileSystem.SharpCompress.Tar;

/// <summary>
/// Specifies the compression type to apply to the tar archive.
/// </summary>
/// <remarks>
/// <para>
/// This enumeration determines if and how the tar archive should be compressed.
/// Compression is applied to the entire archive, not individual entries.
/// </para>
/// <para>
/// Note that compression level (if specified) only affects <see cref="GZip"/>.
/// <see cref="BZip2"/> does not support compression levels.
/// </para>
/// </remarks>
public enum TarPackTo
{
    /// <summary>
    /// Use GZip compression (deflate-based).
    /// Supports compression levels.
    /// Results in .tar.gz or .tgz files.
    /// </summary>
    GZip,

    /// <summary>
    /// Use BZip2 compression.
    /// Does not support compression levels.
    /// Results in .tar.bz2 files.
    /// </summary>
    BZip2,

    /// <summary>
    /// No compression, plain tar archive.
    /// Results in .tar files.
    /// </summary>
    None
}