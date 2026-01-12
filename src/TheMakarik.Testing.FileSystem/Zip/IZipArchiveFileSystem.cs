using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using JetBrains.Annotations;

namespace TheMakarik.Testing.FileSystem.Zip;

[PublicAPI]
public interface IZipArchiveFileSystem : IDisposable, IEnumerable<string>
{
    public string Root { get; }
    public ZipArchive ZipEntry { get; }
    public IZipArchiveAssertion Should();
}