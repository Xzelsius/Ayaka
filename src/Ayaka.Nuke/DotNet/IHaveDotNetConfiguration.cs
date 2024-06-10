// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNet;

using global::Nuke.Common;

/// <summary>
///     Provides information about the .NET build configuration in a <see cref="INukeBuild" />.
/// </summary>
public interface IHaveDotNetConfiguration : IHave
{
    /// <summary>
    ///     Gets the .NET build configuration.
    /// </summary>
    /// <remarks>
    ///     If not overriden, defaults to <see cref="Configuration.Debug" /> locally; otherwise
    ///     <see cref="Configuration.Release" />.
    /// </remarks>
    [Parameter("The .NET build configuration - Default is 'Debug' (local) or 'Release' (server)")]
    [ExcludeFromCodeCoverage]
    Configuration DotNetConfiguration => TryGetValue(() => DotNetConfiguration)
                                         ?? (IsLocalBuild ? Configuration.Debug : Configuration.Release);
}
