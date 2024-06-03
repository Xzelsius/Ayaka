// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke;

using global::Nuke.Common;

/// <summary>
///     Indicates that a <see cref="INukeBuild" /> has a target for cleaning up directories.
/// </summary>
public interface IHaveCleanTarget : IHave
{
    /// <summary>
    ///     Gets the target for cleaning up directories.
    /// </summary>
    Target Clean { get; }
}
