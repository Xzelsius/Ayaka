// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.PublicApi;

using global::Nuke.Common;

/// <summary>
///     Indicates that a <see cref="INukeBuild" /> has a target for shipping the Public API.
/// </summary>
public interface IHaveShipPublicApisTarget
    : IHave
{
    /// <summary>
    ///     Gets the target for shipping the Public API.
    /// </summary>
    Target ShipPublicApis { get; }
}
