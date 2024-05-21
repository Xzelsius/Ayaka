// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNet;

using global::Nuke.Common;

/// <summary>
///     Indicates that a <see cref="INukeBuild" /> has a target for pushing NuGet packages.
/// </summary>
public interface IHaveDotNetPushTarget : IHave
{
    /// <summary>
    ///     Gets the target for pushing NuGet packages.
    /// </summary>
    Target DotNetPush { get; }
}
