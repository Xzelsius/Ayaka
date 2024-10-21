// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.VitePress;

using global::Nuke.Common;

/// <summary>
///     Indicates that a <see cref="INukeBuild" /> has a target for linting a VitePress site.
/// </summary>
public interface IHaveVitePressLintTarget : IHave
{
    /// <summary>
    ///     Gets the target for linting a VitePress site.
    /// </summary>
    Target VitePressLint { get; }
}
