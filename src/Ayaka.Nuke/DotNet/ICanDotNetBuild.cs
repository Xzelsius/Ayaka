// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNet;

using global::Nuke.Common;
using global::Nuke.Common.Tooling;
using global::Nuke.Common.Tools.DotNet;
using global::Nuke.Common.Utilities.Collections;

/// <summary>
///     Provides a target for compiling the current solution using .NET CLI to the <see cref="INukeBuild" />.
/// </summary>
public interface ICanDotNetBuild
    : ICan,
        IHaveSolution,
        IHaveDotNetConfiguration,
        IHaveDotNetRestoreTarget,
        IHaveDotNetBuildTarget
{
    /// <summary>
    ///     Gets the base settings for compiling the current solution.
    /// </summary>
    /// <remarks>
    ///     Applies versioning information if <see cref="IHaveGitVersion" /> is implemented.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    sealed Configure<DotNetBuildSettings> DotNetBuildSettingsBase
        => dotnet => dotnet
            .SetProjectFile(Solution)
            .SetConfiguration(DotNetConfiguration)
            .SetNoRestore(SucceededTargets.Contains(DotNetRestore))
            .When(IsServerBuild, x => x.EnableContinuousIntegrationBuild())
            .WhenNotNull(
                this as IHaveGitVersion,
                (d, o) => d
                    .SetAssemblyVersion(o.Versioning.AssemblySemVer)
                    .SetFileVersion(o.Versioning.AssemblySemFileVer)
                    .SetInformationalVersion(o.Versioning.InformationalVersion));

    /// <summary>
    ///     Gets the additional settings for compiling the current solution.
    /// </summary>
    /// <remarks>
    ///     Override this to provide additional settings for the <see cref="IHaveDotNetBuildTarget.DotNetBuild" /> target.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    Configure<DotNetBuildSettings> DotNetBuildSettings
        => dotnet => dotnet;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    Target IHaveDotNetBuildTarget.DotNetBuild => target => target
        .Description("Compiles the current solution using .NET CLI")
        .Unlisted()
        .DependsOn(DotNetRestore)
        .Executes(() =>
        {
            ReportSummary(
                summary => summary
                    .WhenNotNull(
                        this as IHaveGitVersion,
                        (s, o) => s.AddPair("Version", o.Versioning.NuGetVersionV2))
            );

            _ = DotNetTasks.DotNetBuild(
                dotnet => dotnet
                    .Apply(DotNetBuildSettingsBase)
                    .Apply(DotNetBuildSettings)
            );
        });
}
