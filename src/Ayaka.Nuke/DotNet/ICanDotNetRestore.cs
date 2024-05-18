// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNet;

using global::Nuke.Common;
using global::Nuke.Common.Tooling;
using global::Nuke.Common.Tools.DotNet;

/// <summary>
///     Provides a target for restoring the current solution using .NET CLI to the <see cref="INukeBuild" />.
/// </summary>
public interface ICanDotNetRestore
    : ICan,
        IHaveSolution,
        IHaveDotNetRestoreTarget
{
    /// <summary>
    ///     Gets the base settings for restoring the current solution.
    /// </summary>
    sealed Configure<DotNetRestoreSettings> DotNetRestoreSettingsBase
        => dotnet => dotnet
            .SetProjectFile(Solution);

    /// <summary>
    ///     Gets the additional settings for restoring the current solution.
    /// </summary>
    /// <remarks>
    ///     Override this to provide additional settings for the <see cref="IHaveDotNetRestoreTarget.DotNetRestore" /> target.
    /// </remarks>
    Configure<DotNetRestoreSettings> DotNetRestoreSettings
        => dotnet => dotnet;

    /// <inheritdoc />
    Target IHaveDotNetRestoreTarget.DotNetRestore => target => target
        .Description("Restores the current solution using .NET CLI")
        .Unlisted()
        .TryDependsOn<IHaveCleanTarget>()
        .Executes(() =>
        {
            _ = DotNetTasks.DotNetRestore(
                dotnet => dotnet
                    .Apply(DotNetRestoreSettingsBase)
                    .Apply(DotNetRestoreSettings)
            );
        });
}
