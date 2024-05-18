// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNet;

using global::Nuke.Common;

/// <summary>
///     Indicates that a <see cref="INukeBuild" /> has a target for testing .NET.
/// </summary>
public interface IHaveDotNetTestTarget : IHave
{
    /// <summary>
    ///     Gets the target for testing .NET.
    /// </summary>
    Target DotNetTest { get; }
}
