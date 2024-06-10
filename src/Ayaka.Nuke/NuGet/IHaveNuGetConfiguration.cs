// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.NuGet;

using global::Nuke.Common;

/// <summary>
///     Provides information about the NuGet configuration in a <see cref="INukeBuild" />.
/// </summary>
public interface IHaveNuGetConfiguration : IHave
{
    /// <summary>
    ///     The default NuGet server URL.
    /// </summary>
    public const string DefaultNuGetSource = "https://api.nuget.org/v3/index.json";

    /// <summary>
    ///     Gets the NuGet server URL.
    /// </summary>
    [Parameter("The NuGet server URL - Default is " + DefaultNuGetSource)]
    [ExcludeFromCodeCoverage]
    string NuGetSource => TryGetValue(() => NuGetSource) ?? DefaultNuGetSource;

    /// <summary>
    ///     Gets the NuGet API key.
    /// </summary>
    [Parameter("The NuGet API key")]
    [Secret]
    [ExcludeFromCodeCoverage]
    string? NuGetApiKey => TryGetValue(() => NuGetApiKey);
}
