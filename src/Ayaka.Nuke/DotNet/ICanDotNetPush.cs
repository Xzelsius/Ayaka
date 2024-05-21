// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNet;

using Ayaka.Nuke.NuGet;
using global::Nuke.Common;
using global::Nuke.Common.IO;
using global::Nuke.Common.Tooling;
using global::Nuke.Common.Tools.DotNet;

/// <summary>
///     Provides a target for pushing NuGet packages using .NET CLI to the <see cref="INukeBuild" />.
/// </summary>
public interface ICanDotNetPush
    : ICan,
        IHavePackageArtifacts,
        IHaveNuGetConfiguration,
        IHaveDotNetPackTarget,
        IHaveDotNetPushTarget
{
    /// <summary>
    ///     Gets the base settings for pushing NuGet packages.
    /// </summary>
    sealed Configure<DotNetNuGetPushSettings> DotNetPushSettingsBase
        => dotnet => dotnet
            .SetSource(NuGetSource)
            .SetApiKey(NuGetApiKey)
            .SetSkipDuplicate(skipDuplicate: true);

    /// <summary>
    ///     Gets the additional settings for pushing NuGet packages.
    /// </summary>
    /// <remarks>
    ///     Override this to provide additional settings for the <see cref="IHaveDotNetPushTarget.DotNetPush" /> target.
    /// </remarks>
    Configure<DotNetNuGetPushSettings> DotNetPushSettings
        => dotnet => dotnet;

    /// <summary>
    ///     Gets the base settings for pushing a specific NuGet package.
    /// </summary>
    sealed Configure<DotNetNuGetPushSettings, AbsolutePath> DotNetPushPackageSettingsBase
        => (dotnet, packagePath) => dotnet
            .SetTargetPath(packagePath);

    /// <summary>
    ///     Gets the additional settings for pushing a specific NuGet package.
    /// </summary>
    /// <remarks>
    ///     Override this to provide additional settings for the <see cref="IHaveDotNetPushTarget.DotNetPush" /> target.
    /// </remarks>
    Configure<DotNetNuGetPushSettings, AbsolutePath> DotNetPushPackageSettings
        => (dotnet, packagePath) => dotnet;

    /// <summary>
    ///     Gets all NuGet packages.
    /// </summary>
    /// <remarks>
    ///     If not overridden, all <c>*.nupkg</c> files in the <see cref="IHavePackageArtifacts.PackagesDirectory" /> are
    ///     considered packages.
    ///     <para>
    ///         Please note that symbols packages present in the package directory are pushed automatically.
    ///     </para>
    /// </remarks>
    IEnumerable<AbsolutePath> NuGetPackages => PackagesDirectory.GlobFiles("*.nupkg");

    /// <inheritdoc />
    Target IHaveDotNetPushTarget.DotNetPush => target => target
        .Description("Pushes all NuGet packages using .NET CLI")
        .Unlisted()
        .After<IHaveDotNetPackTarget>()
        .Executes(() =>
        {
            _ = DotNetTasks.DotNetNuGetPush(
                dotnet => dotnet
                    .Apply(DotNetPushSettingsBase)
                    .Apply(DotNetPushSettings)
                    .CombineWith(NuGetPackages, (d, f) => d
                        .Apply(DotNetPushPackageSettingsBase, f)
                        .Apply(DotNetPushPackageSettings, f)),
                degreeOfParallelism: 5,
                completeOnFailure: true);
        });
}