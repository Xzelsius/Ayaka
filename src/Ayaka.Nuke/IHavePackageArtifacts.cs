// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke;

using global::Nuke.Common;
using global::Nuke.Common.IO;

/// <summary>
///     Provides information about the directory where package artifacts are to be dropped in a <see cref="INukeBuild" />.
/// </summary>
public interface IHavePackageArtifacts : IHaveArtifacts
{
    /// <summary>
    ///     Gets the absolute path to the directory where package artifacts are to be dropped.
    /// </summary>
    /// <remarks>
    ///     If not overriden, defaults to <c><see cref="IHaveArtifacts.ArtifactsDirectory" />/packages</c>.
    /// </remarks>
    AbsolutePath PackagesDirectory => ArtifactsDirectory / "packages";
}
