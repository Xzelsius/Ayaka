// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke;

using global::Nuke.Common;

/// <summary>
///     Indicates that a <see cref="INukeBuild" /> has a target for testing.
/// </summary>
public interface IHaveTestTarget : IHave
{
    /// <summary>
    ///     Gets the target for testing.
    /// </summary>
    Target Test { get; }
}
