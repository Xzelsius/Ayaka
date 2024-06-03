// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNetValidate;

using global::Nuke.Common;

/// <summary>
///     Indicates that a <see cref="INukeBuild" /> has a target for validating NuGet packages.
/// </summary>
public interface IHaveDotNetValidateTarget
    : IHave
{
    /// <summary>
    ///     Gets the target for validating NuGet packages.
    /// </summary>
    Target DotNetValidate { get; }
}
