// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNet;

using global::Nuke.Common;

/// <summary>
///     Indicates that a <see cref="INukeBuild" /> has a target for compiling .NET.
/// </summary>
public interface IHaveDotNetBuildTarget : IHave
{
    /// <summary>
    ///     Gets the target for compiling .NET.
    /// </summary>
    Target DotNetBuild { get; }
}
