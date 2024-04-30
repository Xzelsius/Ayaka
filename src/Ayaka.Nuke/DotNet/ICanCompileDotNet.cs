// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNet;

using global::Nuke.Common;
using global::Nuke.Common.Tooling;
using global::Nuke.Common.Tools.DotNet;
using global::Nuke.Common.Utilities.Collections;
using static global::Nuke.Common.Tools.DotNet.DotNetTasks;

/// <summary>
///     Provides a target for compiling the current solution using .NET CLI to the <see cref="INukeBuild" />.
/// </summary>
public interface ICanCompileDotNet
    : ICan,
        IHaveSolution,
        IHaveConfiguration,
        IHaveRestoreTarget,
        IHaveCompileTarget
{
    /// <summary>
    ///     Gets the base settings for compiling the current solution.
    /// </summary>
    /// <remarks>
    ///     Applies versioning information if <see cref="IHaveGitVersion" /> is implemented.
    /// </remarks>
    sealed Configure<DotNetBuildSettings> CompileSettingsBase
        => dotnet => dotnet
            .SetProjectFile(Solution)
            .SetConfiguration(Configuration)
            .SetNoRestore(InvokedTargets.Contains(Restore))
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
    ///     Override this to provide additional settings for the <see cref="IHaveCompileTarget.Compile" /> target.
    /// </remarks>
    Configure<DotNetBuildSettings> CompileSettings
        => dotnet => dotnet;

    /// <inheritdoc />
    Target IHaveCompileTarget.Compile => target => target
        .Description("Compiles the current solution using .NET CLI")
        .DependsOn(Restore)
        .WhenSkipped(DependencyBehavior.Skip)
        .Executes(() =>
        {
            ReportSummary(
                summary => summary
                    .WhenNotNull(
                        this as IHaveGitVersion,
                        (s, o) => s.AddPair("Version", o.Versioning.NuGetVersionV2))
            );

            _ = DotNetBuild(
                dotnet => dotnet
                    .Apply(CompileSettingsBase)
                    .Apply(CompileSettings)
            );
        });
}
