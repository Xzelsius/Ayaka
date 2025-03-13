// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.VitePress;

using global::Nuke.Common;
using global::Nuke.Common.Tooling;
using global::Nuke.Common.Tools.Npm;

/// <summary>
///     Provides a target for installing the VitePress site dependencies to the <see cref="INukeBuild" />.
/// </summary>
public interface ICanVitePressInstall
    : ICan,
        IHaveDocumentation,
        IHaveVitePressInstallTarget
{
    /// <summary>
    ///     Gets the base settings for installing the VitePress site dependencies.
    /// </summary>
    [ExcludeFromCodeCoverage]
    sealed Configure<NpmCiSettings> VitePressInstallSettingsBase
        => install => install
            .SetProcessWorkingDirectory(DocsDirectory)
            .SetProcessOutputLogging(Verbosity == Verbosity.Verbose);

    /// <summary>
    ///     Gets the additional settings for installing the VitePress site dependencies.
    /// </summary>
    /// <remarks>
    ///     Override this to provide additional settings for the <see cref="IHaveVitePressInstallTarget.VitePressInstall" />
    /// </remarks>
    [ExcludeFromCodeCoverage]
    Configure<NpmCiSettings> VitePressInstallSettings
        => install => install;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    Target IHaveVitePressInstallTarget.VitePressInstall => target => target
        .Description("Installs the VitePress site dependencies")
        .Unlisted()
        .Executes(() =>
        {
            _ = NpmTasks.NpmCi(
                npm => npm
                    .Apply(VitePressInstallSettingsBase)
                    .Apply(VitePressInstallSettings));
        });
}
