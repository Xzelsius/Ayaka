// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.VitePress;

using global::Nuke.Common;

/// <summary>
///     Indicates that a <see cref="INukeBuild" /> has a target for building a VitePress site.
/// </summary>
public interface IHaveVitePressBuildTarget : IHave
{
    /// <summary>
    ///     Gets the target for building a VitePress site.
    /// </summary>
    Target VitePressBuild { get; }
}
