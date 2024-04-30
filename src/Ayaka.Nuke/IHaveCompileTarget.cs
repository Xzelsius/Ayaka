// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke;

using global::Nuke.Common;

/// <summary>
///     Indicates that a <see cref="INukeBuild" /> has a target for compiling.
/// </summary>
public interface IHaveCompileTarget : IHave
{
    /// <summary>
    ///     Gets the target for compiling.
    /// </summary>
    Target Compile { get; }
}
