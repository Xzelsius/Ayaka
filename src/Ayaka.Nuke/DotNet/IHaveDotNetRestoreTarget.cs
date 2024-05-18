// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNet;

using global::Nuke.Common;

/// <summary>
///     Indicates that a <see cref="INukeBuild" /> has a target for restoring .NET dependencies.
/// </summary>
public interface IHaveDotNetRestoreTarget : IHave
{
    /// <summary>
    ///     Gets the target for restoring .NET dependencies.
    /// </summary>
    Target DotNetRestore { get; }
}
