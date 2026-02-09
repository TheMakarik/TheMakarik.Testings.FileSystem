using System;
using System.IO;

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
    
    /// <summary>
    /// Generates the function to create file system name using <see cref="NameGenerationType"/> enum
    /// </summary>
    /// <param name="generationType">Enum value to create the function</param>
    public static Func<NamingInfo, string> CreateGeneratingFunction(NameGenerationType generationType)
    {
        return generationType switch
        {
            NameGenerationType.ExtensionAndCount => (info) =>
                info.Extension + (info.Count == 0 ? string.Empty : $"({info.Count})"),
            NameGenerationType.ExtensionAndExtensionCount => (info) =>
            {
                var count = info.GetExtensionCount();
                return info.Extension + (count == 0 ? string.Empty : $"({count})");
            },
            NameGenerationType.RandomName => (info) => GetRandomName() + info.Extension,
            NameGenerationType.RandomNameAndCount => (info) =>
                GetRandomName() + info.Extension + (info.Count == 0 ? string.Empty : $"({info.Count})"),
            NameGenerationType.RandomNameAndExtensionCount => (info) =>
            {
                var count = info.GetExtensionCount();
                return GetRandomName() + info.Extension + (count == 0 ? string.Empty : $"({count})");
            },
            NameGenerationType.RandomNumber => (info) =>
            {
                var random = info.RandomSeed is null ? new Random() : new Random(info.RandomSeed.Value);
                return random.Next(int.MinValue, int.MaxValue).ToString();
            },
            _ => throw new ArgumentOutOfRangeException(nameof(generationType), generationType, null)
        };
    }
    
    private static string GetRandomName()
    {
        return Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
    }
}