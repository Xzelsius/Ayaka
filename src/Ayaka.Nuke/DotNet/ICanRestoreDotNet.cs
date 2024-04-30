// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNet;

using global::Nuke.Common;
using global::Nuke.Common.Tooling;
using global::Nuke.Common.Tools.DotNet;
using static global::Nuke.Common.Tools.DotNet.DotNetTasks;

/// <summary>
///     Provides a target for restoring the current solution using .NET CLI to the <see cref="INukeBuild" />.
/// </summary>
public interface ICanRestoreDotNet
    : ICan,
        IHaveSolution,
        IHaveRestoreTarget
{
    /// <summary>
    ///     Gets the base settings for restoring the current solution.
    /// </summary>
    sealed Configure<DotNetRestoreSettings> RestoreSettingsBase
        => dotnet => dotnet
            .SetProjectFile(Solution);

    /// <summary>
    ///     Gets the additional settings for restoring the current solution.
    /// </summary>
    /// <remarks>
    ///     Override this to provide additional settings for the <see cref="IHaveRestoreTarget.Restore" /> target.
    /// </remarks>
    Configure<DotNetRestoreSettings> RestoreSettings
        => dotnet => dotnet;

    /// <inheritdoc />
    Target IHaveRestoreTarget.Restore => target => target
        .Description("Restores the current solution using .NET CLI")
        .After<IHaveCleanTarget>()
        .Executes(() =>
        {
            _ = DotNetRestore(
                dotnet => dotnet
                    .Apply(RestoreSettingsBase)
                    .Apply(RestoreSettings)
            );
        });
}
