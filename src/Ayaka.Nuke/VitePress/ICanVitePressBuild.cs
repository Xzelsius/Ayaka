// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.VitePress;

using global::Nuke.Common;
using global::Nuke.Common.Tooling;
using global::Nuke.Common.Tools.Npm;

/// <summary>
///     Provides a target for building a VitePress site to the <see cref="INukeBuild" />.
/// </summary>
public interface ICanVitePressBuild
    : ICan,
        IHaveDocumentation,
        IHaveDocumentationArtifacts,
        IHaveVitePressInstallTarget,
        IHaveVitePressBuildTarget
{
    /// <summary>
    ///     Gets the base settings for building the VitePress site.
    /// </summary>
    [ExcludeFromCodeCoverage]
    sealed Configure<NpmRunSettings> VitePressBuildSettingsBase
        => run => run
            .SetProcessWorkingDirectory(DocsDirectory)
            .SetCommand("build")
            .SetArguments(
                "--outDir",
                DocsArtifactsDirectory)
            .SetProcessLogOutput(Verbosity == Verbosity.Verbose);

    /// <summary>
    ///     Gets the additional settings for building the VitePress site.
    /// </summary>
    /// <remarks>
    ///     Override this to provide additional settings for the <see cref="IHaveVitePressBuildTarget.VitePressBuild" />
    ///     target.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    Configure<NpmRunSettings> VitePressBuildSettings
        => run => run;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    Target IHaveVitePressBuildTarget.VitePressBuild => target => target
        .Description("Builds the VitePress site")
        .Unlisted()
        .DependsOn<IHaveVitePressInstallTarget>()
        .Executes(() =>
        {
            _ = NpmTasks.NpmRun(
                npm => npm
                    .Apply(VitePressBuildSettingsBase)
                    .Apply(VitePressBuildSettings));
        });
}
