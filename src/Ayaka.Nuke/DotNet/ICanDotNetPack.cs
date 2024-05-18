// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNet;

using global::Nuke.Common;
using global::Nuke.Common.IO;
using global::Nuke.Common.Tooling;
using global::Nuke.Common.Tools.DotNet;
using global::Nuke.Common.Utilities.Collections;

/// <summary>
///     Provides a target for packing the current solution using .NET CLI to the <see cref="INukeBuild" />.
/// </summary>
public interface ICanDotNetPack
    : ICan,
        IHavePackageArtifacts,
        IHaveSolution,
        IHaveDotNetConfiguration,
        IHaveDotNetBuildTarget,
        IHaveDotNetPackTarget
{
    /// <summary>
    ///     Gets the base settings for packing the current solution.
    /// </summary>
    sealed Configure<DotNetPackSettings> DotNetPackSettingsBase
        => dotnet => dotnet
            .SetProject(Solution)
            .SetConfiguration(DotNetConfiguration)
            .SetNoBuild(SucceededTargets.Contains(DotNetBuild))
            .SetOutputDirectory(PackagesDirectory)
            .WhenNotNull(
                this as IHaveGitRepository,
                (d, o) => d.SetRepositoryUrl(o.GitRepository.HttpsUrl))
            .WhenNotNull(
                this as IHaveGitVersion,
                (d, o) => d.SetVersion(o.Versioning.NuGetVersionV2));

    /// <summary>
    ///     Gets the additional settings for packing the current solution.
    /// </summary>
    /// <remarks>
    ///     Override this to provide additional settings for the <see cref="IHaveDotNetPackTarget.DotNetPack" />.
    /// </remarks>
    Configure<DotNetPackSettings> DotNetPackSettings
        => dotnet => dotnet;

    /// <inheritdoc />
    Target IHaveDotNetPackTarget.DotNetPack => target => target
        .Description("Packs the current solution using .NET CLI")
        .Unlisted()
        .DependsOn(DotNetBuild)
        .WhenNotNull(
            this as IHaveDotNetTestTarget,
            (t, o) => t.After(o.DotNetTest))
        .Produces(PackagesDirectory / "*.nupkg")
        .Executes(() =>
        {
            _ = DotNetTasks.DotNetPack(
                dotnet => dotnet
                    .Apply(DotNetPackSettingsBase)
                    .Apply(DotNetPackSettings)
            );

            ReportSummary(
                summary => summary
                    .AddPair("Packages", PackagesDirectory.GlobFiles("*.nupkg").Count.ToString()));
        });
}
