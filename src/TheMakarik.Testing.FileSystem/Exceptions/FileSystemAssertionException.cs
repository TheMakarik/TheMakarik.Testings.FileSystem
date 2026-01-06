using System;

namespace TheMakarik.Testing.FileSystem.Exceptions;

/// <summary>
/// Represent default exception that occurs then your file system assertion is not true.
/// </summary>
public sealed class FileSystemAssertionException : Exception
{
    /// <summary>
    /// Unparameterized constructor for <see cref="FileSystemAssertionException"/>
    /// </summary>
    public FileSystemAssertionException() : base()
    {
    }

    /// <summary>
    /// Constructor for <see cref="FileSystemAssertionException"/> with exception message
    /// </summary>
    /// <param name="message">Exception message</param>
    public FileSystemAssertionException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructor for <see cref="FileSystemAssertionException"/> with exception message and inner <see cref="Exception"/>
    /// </summary>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Inner <see cref="Exception"/></param>
    public FileSystemAssertionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
