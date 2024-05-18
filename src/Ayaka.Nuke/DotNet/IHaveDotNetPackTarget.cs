// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNet;

using global::Nuke.Common;

/// <summary>
///     Indicates that a <see cref="INukeBuild" /> has a target for packing .NET.
/// </summary>
public interface IHaveDotNetPackTarget
{
    /// <summary>
    ///     Gets the target for packing up .NET.
    /// </summary>
    Target DotNetPack { get; }
}
