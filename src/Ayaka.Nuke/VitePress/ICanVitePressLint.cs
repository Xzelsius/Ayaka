// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.VitePress;

using global::Nuke.Common;
using global::Nuke.Common.Tooling;
using global::Nuke.Common.Tools.Npm;

/// <summary>
///     Provides a target for linting a VitePress site to the <see cref="INukeLint" />.
/// </summary>
public interface ICanVitePressLint
    : ICan,
        IHaveDocumentation,
        IHaveVitePressInstallTarget,
        IHaveVitePressLintTarget
{
    /// <summary>
    ///     Gets the base settings for linting the VitePress site.
    /// </summary>
    [ExcludeFromCodeCoverage]
    sealed Configure<NpmRunSettings> VitePressLintSettingsBase
        => run => run
            .SetProcessWorkingDirectory(DocsDirectory)
            .SetCommand("lint")
            .SetProcessLogOutput(Verbosity == Verbosity.Verbose);

    /// <summary>
    ///     Gets the additional settings for linting the VitePress site.
    /// </summary>
    /// <remarks>
    ///     Override this to provide additional settings for the <see cref="IHaveVitePressLintTarget.VitePressLint" />
    ///     target.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    Configure<NpmRunSettings> VitePressLintSettings
        => run => run;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    Target IHaveVitePressLintTarget.VitePressLint => target => target
        .Description("Lints the VitePress site")
        .Unlisted()
        .DependsOn<IHaveVitePressInstallTarget>()
        .TryBefore<IHaveVitePressBuildTarget>()
        .Executes(() =>
        {
            _ = NpmTasks.NpmRun(
                npm => npm
                    .Apply(VitePressLintSettingsBase)
                    .Apply(VitePressLintSettings));
        });
}
