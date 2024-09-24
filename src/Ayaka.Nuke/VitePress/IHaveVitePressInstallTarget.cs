// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.VitePress;

using global::Nuke.Common;

/// <summary>
///     Indicates that a <see cref="INukeBuild" /> has a target for installing VitePress dependencies.
/// </summary>
public interface IHaveVitePressInstallTarget : IHave
{
    /// <summary>
    ///     Gets the target for installing VitePress dependencies.
    /// </summary>
    Target VitePressInstall { get; }
}
