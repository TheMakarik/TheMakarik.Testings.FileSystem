namespace TheMakarik.Testing.FileSystem.AutoNaming;

/// <summary>
/// Predefined strategies for automatic name generation of files and directories.
/// </summary>
public enum NameGenerationType
{
    /// <summary>Random base name + extension (no counter)</summary>
    RandomName,

    /// <summary>Random base name + extension + counter in parentheses when > 0</summary>
    RandomNameAndCount,

    /// <summary>Random base name + extension + real count of files with this extension</summary>
    RandomNameAndExtensionCount,

    /// <summary>Just a random integer (very large range)</summary>
    RandomNumber,

    /// <summary>Only extension + counter when > 0 (file1.txt → file(2).txt, file(3).txt, …)</summary>
    ExtensionAndCount,

    /// <summary>Only extension + real count of files with this extension</summary>
    ExtensionAndExtensionCount,
}