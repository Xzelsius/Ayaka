// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNet;

using System.ComponentModel;
using global::Nuke.Common.Tooling;

/// <summary>
///     Represents the .NET build configuration.
/// </summary>
/// <remarks>
///     See <see href="https://learn.microsoft.com/en-us/visualstudio/ide/understanding-build-configurations" /> for more
///     information regarding build configuration.
/// </remarks>
[TypeConverter(typeof(TypeConverter<Configuration>))]
public class Configuration : Enumeration
{
    /// <summary>
    ///     The <c>Debug</c> build configuration.
    /// </summary>
    public static Configuration Debug = new() { Value = nameof(Debug) };

    /// <summary>
    ///     The <c>Release</c> build configuration.
    /// </summary>
    public static Configuration Release = new() { Value = nameof(Release) };

    /// <summary>
    ///     Converts the <see cref="Configuration" /> to a <see cref="string" />.
    /// </summary>
    /// <param name="configuration">The build configuration to convert.</param>
    public static implicit operator string(Configuration configuration) => configuration.Value;
}
