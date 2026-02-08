using System;

namespace TheMakarik.Testing.FileSystem.AutoNaming;

/// <summary>
/// Configuration of the active name generator: function + current state.
/// </summary>
public class NamingConfiguration
{
    /// <summary>
    /// The function that generates the next name based on current <see cref="NamingInfo"/>.
    /// </summary>
    public Func<NamingInfo, string> GenerateFunction { get; init; } = null!;

    /// <summary>
    /// Current state / counters / context used by the name generator.
    /// </summary>
    public NamingInfo NamingInfo { get; init; } = null!;
}