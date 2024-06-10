// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke;

using global::Nuke.Common;
using global::Nuke.Common.IO;

/// <summary>
///     Provides information about the directory where test artifacts are to be dropped in a <see cref="INukeBuild" />.
/// </summary>
public interface IHaveTestArtifacts : IHaveArtifacts
{
    /// <summary>
    ///     Gets the absolute path to the directory where test artifacts are to be dropped.
    /// </summary>
    /// <remarks>
    ///     If not overriden, defaults to <c><see cref="IHaveArtifacts.ArtifactsDirectory" />/test-results</c>.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    AbsolutePath TestResultsDirectory => ArtifactsDirectory / "test-results";
}
