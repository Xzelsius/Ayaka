// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke;

using global::Nuke.Common;
using global::Nuke.Common.IO;

/// <summary>
///     Provides information about the directory where code coverage reports are dropped in a <see cref="INukeBuild" />.
/// </summary>
public interface IHaveCodeCoverage : IHaveArtifacts
{
    /// <summary>
    ///     Gets the absolute path to the directory where code coverage reports are to be dropped.
    /// </summary>
    /// <remarks>
    ///     If not overriden, defaults to <c><see cref="IHaveArtifacts.ArtifactsDirectory" />/coverage</c>.
    /// </remarks>
    AbsolutePath CoverageDirectory => ArtifactsDirectory / "coverage";
}
