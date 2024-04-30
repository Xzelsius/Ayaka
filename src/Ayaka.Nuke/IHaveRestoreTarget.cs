// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke;

using global::Nuke.Common;

/// <summary>
///     Indicates that a <see cref="INukeBuild" /> has a target for restoring dependencies.
/// </summary>
public interface IHaveRestoreTarget : IHave
{
    /// <summary>
    ///     Gets the target for restoring dependencies.
    /// </summary>
    Target Restore { get; }
}
