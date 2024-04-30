// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNet;

using global::Nuke.Common;

/// <summary>
///     Provides information about the .NET build configuration in a <see cref="INukeBuild" />.
/// </summary>
public interface IHaveConfiguration : IHave
{
    /// <summary>
    ///     Gets the .NET build configuration.
    /// </summary>
    /// <remarks>
    ///     If not overriden, defaults to <see cref="Configuration.Debug" /> locally; otherwise
    ///     <see cref="Configuration.Release" />.
    /// </remarks>
    [Parameter("The .NET build configuration - Default is 'Debug' (local) or 'Release' (server)")]
    Configuration Configuration => TryGetValue(() => Configuration)
                                   ?? (IsLocalBuild ? Configuration.Debug : Configuration.Release);
}
