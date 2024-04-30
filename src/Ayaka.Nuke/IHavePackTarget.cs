// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke;

using global::Nuke.Common;

/// <summary>
///     Indicates that a <see cref="INukeBuild" /> has a target for packing up the application for publishing.
/// </summary>
public interface IHavePackTarget
{
    /// <summary>
    ///     Gets the target for packing up the application.
    /// </summary>
    Target Pack { get; }
}
