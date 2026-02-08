using System;

namespace TheMakarik.Testing.FileSystem.Core;

/// <summary>
/// Default interface for every file system with events
/// </summary>
public interface IDefaultFIleSystemEvents
{
    /// <summary>
    /// Event that occurs then the file system instance starts disposing
    /// </summary>
    public event EventHandler Disposed;

    /// <summary>
    /// Events that occurs then file system  instance starts an assertion
    /// </summary>
    public event EventHandler AssertionStart;
}