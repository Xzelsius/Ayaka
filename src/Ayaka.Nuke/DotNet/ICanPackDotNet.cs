// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNet;

using Ayaka.Nuke;
using global::Nuke.Common;
using global::Nuke.Common.IO;
using global::Nuke.Common.Tooling;
using global::Nuke.Common.Tools.DotNet;
using global::Nuke.Common.Utilities.Collections;
using static global::Nuke.Common.Tools.DotNet.DotNetTasks;

/// <summary>
/// Provides a target for packing the current solution using .NET CLI to the <see cref="INukeBuild" />.
/// </summary>
public interface ICanPackDotNet
    : ICan,
        IHavePackageArtifacts,
        IHaveSolution,
        IHaveConfiguration,
        IHaveCompileTarget,
        IHavePackTarget
{
    /// <summary>
    /// Gets the base settings for packing the current solution.
    /// </summary>
    sealed Configure<DotNetPackSettings> PackSettingsBase
        => dotnet => dotnet
            .SetProject(Solution)
            .SetConfiguration(Configuration)
            .SetNoBuild(SucceededTargets.Contains(Compile))
            .SetOutputDirectory(PackagesDirectory)
            .WhenNotNull(
                this as IHaveGitRepository,
                (d, o) => d.SetRepositoryUrl(o.GitRepository.HttpsUrl))
            .WhenNotNull(
                this as IHaveGitVersion,
                (d, o) => d.SetVersion(o.Versioning.NuGetVersionV2));

    /// <summary>
    /// Gets the additional settings for packing the current solution.
    /// </summary>
    /// <remarks>
    /// Override this to provide additional settings for the <see cref="IHavePackTarget.Pack" /> target.
    /// </remarks>
    Configure<DotNetPackSettings> PackSettings
        => dotnet => dotnet;

    /// <inheritdoc />
    Target IHavePackTarget.Pack => target => target
        .Description("Packs the current solution using .NET CLI")
        .DependsOn(Compile)
        .Produces(PackagesDirectory / "*.nupkg")
        .Executes(() =>
        {
            _ = DotNetPack(
                dotnet => dotnet
                    .Apply(PackSettingsBase)
                    .Apply(PackSettings)
            );

            ReportSummary(
                summary => summary
                    .AddPair("Packages", PackagesDirectory.GlobFiles("*.nupkg").Count.ToString()));
        });
}
