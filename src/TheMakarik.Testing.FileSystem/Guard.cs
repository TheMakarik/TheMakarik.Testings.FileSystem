using System;
using System.Runtime.CompilerServices;

namespace TheMakarik.Testing.FileSystem;

/// <summary>
/// A simple guard that throws exceptions
/// </summary>
[Obsolete("Use ArgumentNullException.ThrowIfNull(value) instead")]
internal static class Guard
{
    /// <summary>
    /// Throws <see cref="System.ArgumentNullException"/> then <see cref="value"/> is null (Nothing at VB.net)
    /// </summary>
    /// <param name="value">value to check</param>
    /// <param name="valueName">value name, DO NOT set this argument, it will be set by default</param>
    /// <exception cref="ArgumentNullException">Throws  then <see cref="value"/> is null (Nothing at VB.net)</exception>
    internal static void AgainstNull(object? value, string valueName)
    {
        if (value is null)
            throw new ArgumentNullException(valueName);
    }
}